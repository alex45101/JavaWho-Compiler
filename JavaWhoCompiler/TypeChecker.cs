using System.Collections;

namespace JavaWhoCompiler
{
    public class TypeException(string message) : Exception(message);

    public class Scope
    {
        public Scope Parent { get; init; }
        private readonly Dictionary<string, TypeBase> lookUp = new();

        public Scope(Scope parent)
        { 
            Parent = parent;
        }

        public void Define(string name, TypeBase type)
        {
            lookUp[name] = type;
        }

        public TypeBase LookUp(string name)
        {
            if (lookUp.TryGetValue(name, out TypeBase value))
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

        public override string ToString() {
            return Name;
        }


        // primitives
        public static PrimitiveType IntPrimitive = new("Int");
        public static PrimitiveType BooleanPrimitive = new("Boolean");
        public static PrimitiveType VoidPrimitive = new("Void");


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
        
        public static HashSet<TypeBase> Predefined = new([
                IntPrimitive,
                BooleanPrimitive,
                VoidPrimitive,
                ObjectBuiltIn,
                StringBuiltIn,
        ]);
    }

    public class PrimitiveType(string name) : TypeBase(name) {
        public override bool CanBeAssignedTo(TypeBase other) {
            return Equals(other);
        }
    }


    public class TypeMap : IEnumerable<TypeBase> {
        private readonly Dictionary<string, TypeBase> types = new();

        public TypeMap() : this([]) {}

        public TypeMap(IEnumerable<TypeBase> predefined) {
            foreach(TypeBase type in predefined) {
                types.Add(type.Name, type);
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

        public T GetTypeAs<T>(string type) 
            where T: TypeBase
        {
            TypeBase typeObj = GetType(type);
            return typeObj switch {
                T classType => classType,
                _ => throw new TypeException($"Type {type} is not a {typeof(T)} type")
            };
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

    public sealed record TypeList(List<TypeBase> Types) {
        public bool AreSubtypesOf(TypeList other) {
            return Types.Count == other.Types.Count &&
                Types.Index().All(
                        indexedType => 
                        indexedType.Item.CanBeAssignedTo(other.Types[indexedType.Index])
                        );
        }

        public bool Equals(TypeList other) {
            return Types.SequenceEqual(other.Types);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            foreach(TypeBase type in Types) {
                hashCode.Add(type);
            }

            return hashCode.ToHashCode();
        }
    }

    public sealed record MethodSignature(string Name, TypeList ParamTypes, TypeBase ReturnType) {
        public bool Equals(MethodSignature other) {
            // signatures will be considered unique by (Name + ParamTypes)
            return Name == other.Name &&
                ParamTypes.Equals(other.ParamTypes);
        }

        // called when two method signatures are 'equal' (name + param types)
        public bool CanOverride(MethodSignature other) {
            return ReturnType.CanBeAssignedTo(other.ReturnType);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            hashCode.Add(Name);

            foreach(TypeBase paramType in ParamTypes.Types) {
                hashCode.Add(paramType);
            }

            return hashCode.ToHashCode();
        }
    }

    public class ClassType : TypeBase
    {
        private List<AST> variableDeclarations;
        private List<AST> methodDefinitions;
        private Constructor constructor { get; }


        public ClassType ParentClassType { get; }

        public Dictionary<string, HashSet<MethodSignature>> MethodSignatures { get; } = new();
        // name to type
        public Dictionary<string, TypeBase> Fields { get; private set; }

        public TypeList ConstructorTypes { get; private set; }

        public bool isChecked = false;


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

            this.constructor = (Constructor)constructor;
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

            InitializeConstructor(typeMap);

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

        private void InitializeConstructor(TypeMap typeMap) {
            ConstructorTypes = new TypeList(
                constructor.Parameters.Select(param => 
                    typeMap.GetType(((VariableDeclaration)param).Type.Value)).ToList()
            );
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
            Fields = ParentClassType is not null ? new(ParentClassType.Fields) : new();

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
                TypeBase newMethodReturnType = TypeBase.VoidPrimitive;
                if(methodDefinition.ReturnType is not null) {
                    newMethodReturnType = typeMap.GetType(methodDefinition.ReturnType.Value);
                }

                MethodSignature newMethodSignature = new(
                    methodDefinition.Name.Value,
                    new TypeList(
                        methodDefinition.Parameters.Select(vd => typeMap.GetType(((VariableDeclaration)vd).Type.Value)).ToList()
                        ),
                        newMethodReturnType
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
        private TypeMap Types = new(TypeBase.Predefined);


        private void CreateClassType(
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
            if(classDefinition.ExtendsName?.Value is string extendsName) {
                if(!definedClasses.ContainsKey(extendsName)) {
                    throw new TypeException($"Inherited class {extendsName} is not defined");
                }

                workingTree.Add(className);
                CreateClassType(extendsName, definedClasses, workingTree);
                extendingClassType = Types.GetType(extendsName);
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

        private void CreateAndInitializeClassTypes(List<AST> classes) {
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
                CreateClassType(classDefinition.Name.Value, definedClasses, workingTree);
                // Types.Add(classDefinition.Name.Value, CreateType(classDefinition.Name.Value, definedClasses, workingTree));
            }

            // third pass: populate class types with all type info
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
                    CreateAndInitializeClassTypes(prog.Classes);

                    foreach(AST classDefinition in prog.Classes) {
                        CheckTypeHelper(classDefinition);
                    }

                    foreach (AST statement in prog.Statements)
                    {
                        CheckTypeHelper(statement);
                    }
                    break;
                case ClassDefinition classDefinition:
                    CheckClass(classDefinition);

                    break;
                case VariableDeclaration varDec:
                    scope.Define(varDec.Var.Value, Types.GetType(varDec.Type.Value));

                    break;
                case AssignmentStatement assignmentStatement:

                    TypeBase leftType = scope.LookUp(assignmentStatement.Var.Value);

                    TypeBase rightType = GetExpressionType(assignmentStatement.Val);

                    if (!rightType.CanBeAssignedTo(leftType))
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

        private void CheckClass(ClassDefinition classDefinition) {
            ClassType classType = Types.GetTypeAs<ClassType>(classDefinition.Name.Value);

            // enter class scope
            EnterScope();

            // add vardecs to scope
            foreach(AST variableDeclaration in classDefinition.VariableDeclarations) {
                CheckTypeHelper(variableDeclaration);
            }

            Constructor constructor = (Constructor)classDefinition.Constructor;
            CheckClassConstructor(constructor, classType);

            foreach(AST methodDefinition in classDefinition.MethodDefinitions) {
                CheckClassMethod((MethodDefinition)methodDefinition);
            }

            // exit class scope
            ExitScope();
        }

        private void CheckClassMethod(MethodDefinition methodDefinition) {
            EnterScope();

            // add params to scope
            foreach(AST variableDeclaration in methodDefinition.Parameters) {
                CheckTypeHelper(variableDeclaration);
            }

            BlockStatement body = (BlockStatement)methodDefinition.Body;

            TypeBase methodReturnType = TypeBase.VoidPrimitive;
            if(methodDefinition.ReturnType is not null) {
                methodReturnType = Types.GetType(methodDefinition.ReturnType.Value);
            }

            bool returned = false;
            for(int i = 0; i < body.Statements.Count; i++) {
                AST statement = body.Statements[i];
                if(statement is ReturnStatement returnStatement) {
                    if(i < body.Statements.Count - 1) {
                        throw new TypeException($"Unreachable code after return in method {methodDefinition.Name.Value}");
                    }
                
                    TypeBase returnExpressionType = TypeBase.VoidPrimitive;
                    if(returnStatement.Val is not null) {
                        returnExpressionType = GetExpressionType(returnStatement.Val);

                    }

                    if(!returnExpressionType.CanBeAssignedTo(methodReturnType)) {
                        throw new TypeException($"Method {methodDefinition.Name.Value} cannot return type {returnExpressionType}");
                    }

                    returned = true;
                } else {
                    CheckTypeHelper(statement);
                }
            }

            if(methodReturnType != TypeBase.VoidPrimitive && !returned) {
                throw new TypeException($"Method {methodDefinition.Name.Value} expects return value of type {methodReturnType} but got none");
            }

            ExitScope();
        }

        private void CheckClassConstructor(Constructor constructor, ClassType classType) {
            // enter constructor scope
            EnterScope();

            // add parameter vardecs to scope
            foreach(AST variableDeclaration in constructor.Parameters) {
                CheckTypeHelper(variableDeclaration);
            }

            // check super call
            if(classType.ParentClassType is not null) {
                List<AST> superArguments = constructor.SuperArguments;
                if(superArguments is null) {
                    throw new TypeException($"Constructor for class {classType.Name} is missing super call");
                }

                TypeList superCallTypes = GetExpressionTypeList(superArguments);
                if(!superCallTypes.AreSubtypesOf(classType.ParentClassType.ConstructorTypes)) {
                    throw new TypeException($"Super call arguments in class {classType.Name} are not compatible with parent class {classType.ParentClassType} constructor");
                }
            } else if(constructor.SuperArguments is not null) {
                throw new TypeException($"Class {classType} attempts to call a super constructor when it does not inherit any class");
            }

            foreach(AST statement in constructor.Statements) {
                CheckTypeHelper(statement);
            }

            // exit constructor scope
            ExitScope();
        }



        private void EnterScope()
        {
            scope = new Scope(scope);
        }

        private void ExitScope()
        {
            scope = scope.Parent;
        }

        private TypeBase GetExpressionType(AST node)
        {
            return node switch 
            { 
                IntLiteral => TypeBase.IntPrimitive,
                StringLiteral => TypeBase.StringBuiltIn,
                BooleanLiteral => TypeBase.BooleanPrimitive,
                IdentifiedNode identifier => scope.LookUp(identifier.Value),
                _ => throw new TypeException($"Cannot obtain type of {node}")
            };
        }

        private TypeList GetExpressionTypeList(List<AST> nodes) {
            return new TypeList(nodes.Select(GetExpressionType).ToList());
        }
    }
}
