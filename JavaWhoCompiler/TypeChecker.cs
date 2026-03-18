using System.Collections;

namespace JavaWhoCompiler
{
    public class TypeException(string message) : Exception(message);

    public class Scope
    {
        public Scope Parent { get; init; }
        private readonly Dictionary<string, string> lookUp = new();

        public Scope(Scope parent)
        { 
            Parent = parent;
        }

        public void Define(string name, string type)
        {
            lookUp[name] = type;
        }

        public string LookUp(string name)
        {
            if (lookUp.TryGetValue(name, out string value))
            {
                return value;
            }

            if (Parent != null)
            { 
                return Parent.LookUp(name);
            }

            throw new TypeException($"Undefined variable {name}");
        }
    }

    public abstract class TypeBase(string name) {
        public string Name { get; } = name;

        public abstract bool CanBeAssignedTo(TypeBase other);


        // primitives
        public static PrimitiveType IntPrimitive = new("Int");
        public static PrimitiveType BooleanPrimitive = new("Boolean");
        public static PrimitiveType VoidPrimitive = new("Void");

        public static HashSet<PrimitiveType> Primitives = new([
                IntPrimitive,
                BooleanPrimitive,
                VoidPrimitive,
        ]);

        // built ins
        public static ClassType ObjectBuiltIn = new(
                "Object",
                [], // vardecs
                new Constructor([], null, []),
                [], // methods
                null // extending class
                );

        public static ClassType StringBuiltIn = new(
                "String",
                [],
                new Constructor([], null, []),
                [], // methods
                ObjectBuiltIn // extending class
                );
    }

    public class PrimitiveType(string name) : TypeBase(name) {
        public override bool CanBeAssignedTo(TypeBase other) {
            return Equals(other);
        }
    }


    public class TypeMap : IEnumerable<TypeBase> {
        private readonly Dictionary<string, TypeBase> types = new();

        public TypeMap() : this([]) {}

        public TypeMap(IEnumerable<PrimitiveType> primitives) {
            foreach(PrimitiveType primitive in primitives) {
                types.Add(primitive.Name, primitive);
            }
        }

        public void DefineType(TypeBase classDefinition) {
            AssertNotDefined(classDefinition.Name);

            types.Add(classDefinition.Name, classDefinition);
        }

        public bool CanBeAssignedTo(string assigningTypeName, string targetTypeName) {
            TypeBase targetType =  GetType(targetTypeName);
            TypeBase assigningType = GetType(assigningTypeName);

            return assigningType.CanBeAssignedTo(targetType);
        }

        public void AssertCanAssign(string assigningTypeName, string targetTypeName) {
            if(!CanBeAssignedTo(assigningTypeName, targetTypeName)) {
                throw new TypeException($"Type {assigningTypeName} cannot be assigned to type {targetTypeName}");
            }
        }

        public bool TypeDefined(string type) {
            return types.ContainsKey(type);
        }

        public void AssertNotDefined(string type) {
            if(TypeDefined(type)) {
                throw new TypeException($"Type {type} is already defined");
            }
        }

        public void AssertDefined(string type) {
            if(!TypeDefined(type)) {
                throw new TypeException($"Type {type} is not defined");
            }
        }

        public TypeBase GetType(string type) { 
            AssertDefined(type);
            return types[type];
        }

        public IEnumerator<TypeBase> GetEnumerator() {
            foreach((_, TypeBase type) in types)
            {
                yield return type;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }


    // public class TypeList(IEnumerable<TypeBase> types) : List<TypeBase>(types) {
    //
    //     public static TypeList FromASTNodes(List<AST> variableDeclarations) {
    //         return new TypeList(variableDeclarations.Select(vd => {
    //                         throw new NotImplementedException();
    //                         return TypeBase.IntPrimitive;
    //                     }));
    //     }
    //
    //     public override bool Equals(Object other) {
    //         return other is TypeList paramTypes &&
    //                 this.SequenceEqual(paramTypes);
    //     }
    //
    //     public override int GetHashCode() {
    //         HashCode hashCode = new();
    //         foreach(TypeBase paramType in this) {
    //             hashCode.Add(paramType.GetHashCode());
    //         }
    //
    //         return hashCode.ToHashCode();
    //     }
    // }

    public sealed record MethodSignature(string Name, List<TypeBase> ParamTypes, TypeBase ReturnType) {
        public bool Equals(MethodSignature other) {
            // signatures will be considered unique by (Name + ParamTypes)
            return Name == other.Name &&
                ParamTypes.SequenceEqual(other.ParamTypes);
        }

        // called when two method signatures are 'equal' (name + param types)
        public bool CanOverride(MethodSignature other) {
            return ReturnType.CanBeAssignedTo(other.ReturnType);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            hashCode.Add(Name);

            foreach(TypeBase paramType in ParamTypes) {
                hashCode.Add(paramType);
            }

            return hashCode.ToHashCode();
        }
    }

    public class ClassType : TypeBase
    {
        ClassType ParentClassType { get; }
        public Constructor Constructor { get; }

        public Dictionary<string, HashSet<MethodSignature>> MethodSignatures { get; } = new();

        // name to type
        public Dictionary<string, TypeBase> Fields { get; private set; }

        public bool isChecked = false;

        private List<AST> variableDeclarations;
        private List<AST> methodDefinitions;


        public ClassType(
            ClassDefinition classDefinition,
            TypeBase parentClassType
            ) 
            : this(classDefinition.Name.Value,
                    classDefinition.VariableDeclarations,
                    classDefinition.Constructor,
                    classDefinition.MethodDefinitions,
                    parentClassType) {}


        public ClassType(
                string className,
                List<AST> variableDeclarations,
                AST constructor,
                List<AST> methodDefinitions,
                TypeBase parentClassType
                )
                : base(className)
        {
            if(parentClassType is ClassType classType) {
                ParentClassType = classType;
            } else if(parentClassType is PrimitiveType primitiveType){
                throw new TypeException($"Cannot extend class by primitive type ${primitiveType.Name}");
            }

            this.variableDeclarations = variableDeclarations;
            this.methodDefinitions = methodDefinitions;

            // InitializeLocalFields(variableDeclarations);

            Constructor = (Constructor)constructor;
            // InitializeMethodSignatures(methodDefinitions);
        }
        
        public override bool CanBeAssignedTo(TypeBase other) {
            switch(other) {
                case ClassType classType: {
                    if(classType.Name == Name) return true;

                    if(ParentClassType is null) return false;

                    return ParentClassType.CanBeAssignedTo(classType);
                }

                default:
                  return false;
            }
        }

        public bool CanAccessField(string fieldName) {
            return Fields.ContainsKey(fieldName) ||
                   (
                    ParentClassType is not null &&
                    ParentClassType.CanAccessField(fieldName)
                   );
        }

        public void PopulateWithTypeMap(TypeMap typeMap) {
            if(isChecked) return;

            // populate parent class first
            if(ParentClassType is not null) {
                ParentClassType.PopulateWithTypeMap(typeMap);
            }

            InitializeFields(typeMap);

            InitializeLocalMethodSignatures(typeMap);
            InheritMethods();

            isChecked = true;
        }

        private void MergeMatchingParentMethodSet(HashSet<MethodSignature> parentMethodSet, HashSet<MethodSignature> localMethodSet) {
            // local class has matching method name to parent
            foreach(MethodSignature parentMethodSignature in parentMethodSet) {
                if(localMethodSet.TryGetValue(parentMethodSignature, out MethodSignature localMethodSignature)) {
                    // a local method is trying to override a parent method
                    if(!localMethodSignature.CanOverride(parentMethodSignature)) {
                        throw new TypeException($"Overriding method {localMethodSignature.Name}'s return type " + 
                                $"{localMethodSignature.ReturnType} is not a subtype of the parent method's " +
                                $"return type {parentMethodSignature.ReturnType}");
                    }
                } else {
                    // no override needed, add parent method signature to local set
                    localMethodSet.Add(parentMethodSignature);
                }
            }
        }

        private void InheritMethods() {
            if(ParentClassType is null) return;

            foreach((string parentMethodName, HashSet<MethodSignature> parentMethodSet) in ParentClassType.MethodSignatures) {
                if(MethodSignatures.TryGetValue(parentMethodName, out HashSet<MethodSignature> localMethodSet)) {
                    MergeMatchingParentMethodSet(parentMethodSet, localMethodSet);
                } else {
                    // local class doesn't have any matching method names to parent
                    MethodSignatures.Add(parentMethodName, parentMethodSet);
                }
            }
        }

        // private void InitializeLocalFields(List<AST> variableDeclarations) {
        //     foreach(VariableDeclaration variableDeclaration in variableDeclarations) {
        //         if(CanAccessField(variableDeclaration.Var.Value)) {
        //             throw new TypeException($"Redelcaration of field {variableDeclaration.Var.Value}");
        //         }
        //
        //         LocalClassFields.Add(
        //                 variableDeclaration.Var.Value,
        //                 variableDeclaration.Type.Value
        //                 );
        //     }
        // }

        private void InitializeFields(TypeMap typeMap) {
            Fields = ParentClassType is not null ? ParentClassType.Fields : new();

            foreach(VariableDeclaration variableDeclaration in variableDeclarations) {
                if(Fields.ContainsKey(variableDeclaration.Var.Value)) {
                    throw new TypeException($"Redeclaration of field {variableDeclaration.Var.Value}");
                }

                Fields.Add(
                        variableDeclaration.Var.Value,
                        typeMap.GetType(variableDeclaration.Type.Value)
                        );
            }
        }

        private void InitializeLocalMethodSignatures(TypeMap typeMap) {
            foreach(MethodDefinition methodDefinition in methodDefinitions) {
                MethodSignature newMethodSignature = new MethodSignature(
                    methodDefinition.Name.Value,
                    methodDefinition.Parameters.Select(vd => typeMap.GetType(((VariableDeclaration)vd).Type.Value)).ToList(),
                    typeMap.GetType(methodDefinition.ReturnType.Value)
                );
                
                if(MethodSignatures.TryGetValue(newMethodSignature.Name, out HashSet<MethodSignature> methodSetWithSameName)) {
                    if(methodSetWithSameName.Contains(newMethodSignature)) {
                        // exact signature match, local redeclaration
                        throw new TypeException($"Redeclaration of method {newMethodSignature}");
                    }
                }
            }
        }
    }

    public class TypeChecker
    {
        private Scope scope = new(null);
        private TypeMap Types = new(TypeBase.Primitives);


        private void CreateType(
            string className, 
            Dictionary<string, ClassDefinition> definedClasses,
            HashSet<string> workingTree) 
        {
            if(workingTree.Contains(className)) {
                // cyclic inheritance
                throw new TypeException($"Class {className} is part of an inheritance cycle");
            }

            if(Types.TypeDefined(className)) {
                return;
            }

            if(!definedClasses.TryGetValue(className, out ClassDefinition classDefinition)) {
                throw new TypeException($"Class {className} is not defined");
            }


            TypeBase extendingClassType = null;
            if(classDefinition.ExtendsName.Value is string extendsName) {
                if(!Types.TypeDefined(extendsName)) {
                    throw new TypeException($"Inherited class {extendsName} is not defined");
                }

                workingTree.Add(className);
                CreateType(extendsName, definedClasses, workingTree);
            }

            Types.DefineType(
                new ClassType(
                    classDefinition,
                    extendingClassType
                )
            );
        }

        // private void ValidateClass(ClassType classType) {
        //     // ensures fields are valid types
        //     foreach((_, string typeName) in classType.LocalClassFields) {
        //         if(!Types.ContainsKey(typeName)) {
        //             throw new TypeException($"Type {typeName} is not defined");
        //         }
        //     }
        //
        //
        // }

        private void CreateAndCheckTypes(List<AST> classes) {
            Dictionary<string, ClassDefinition> definedClasses = new();

            // first pass: add classes to dictionary
            foreach(ClassDefinition classDefinition in classes) {
                if(definedClasses.ContainsKey(classDefinition.Name.Value)) {
                    throw new TypeException($"Class {classDefinition.Name.Value} defined more than once");
                }

                definedClasses.Add(classDefinition.Name.Value, classDefinition);
            }

            // second pass: convert ClassDefinitions into ClassTypes
            foreach(ClassDefinition classDefinition in classes) {
                HashSet<string> workingTree = new();
                CreateType(classDefinition.Name.Value, definedClasses, workingTree);
                // Types.Add(classDefinition.Name.Value, CreateType(classDefinition.Name.Value, definedClasses, workingTree));
            }

            // third pass: validate class types :(
            foreach(TypeBase type in Types) {
                if(type is ClassType classType) {
                    classType.PopulateWithTypeMap(Types);
                }
            }

        }

        public static void CheckType(AST node)
        { 
            TypeChecker typeChecker = new TypeChecker();

            typeChecker.CheckTypeHelper(node);
        }

        private void CheckTypeHelper(AST node)
        {   
            switch (node)
            {
                case ProgramNode prog:
                    CreateAndCheckTypes(prog.Classes);

                    foreach (AST statement in prog.Statements)
                    {
                        CheckTypeHelper(statement);
                    }
                    break;
                case VariableDeclaration varDec:
                    scope.Define(varDec.Var.Value, varDec.Type.Value);

                    break;
                case AssignmentStatement assignmentStatement:

                    string leftType = scope.LookUp(assignmentStatement.Var.Value);

                    string rightType = GetExpressionType(assignmentStatement.Val);

                    if (leftType != rightType)
                    {
                        throw new TypeException($"Can not assign {leftType} to {rightType}");
                    }

                    break;
                case null:
                    throw new TypeException("Null node given");
                default:
                    throw new TypeException($"Type is not supported: {node.GetType()}");
            }
        }

        private void CheckClass(ClassDefinition classdef) {
        }

        private void EnterScope()
        {
            scope = new Scope(scope);
        }

        private void ExitScope()
        {
            scope = scope.Parent;
        }

        private string GetExpressionType(AST node)
        {
            return node switch 
            { 
                IntLiteral => "Int",
                StringLiteral => "String",
                BooleanLiteral => "Boolean",
                _ => scope.LookUp(node.ToString())
            };
        }
    }
}
