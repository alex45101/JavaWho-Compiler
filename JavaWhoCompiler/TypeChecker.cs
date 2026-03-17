using System;
using System.Collections.Generic;
using System.Text;

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

    public sealed record MethodSignature(string Name, List<string> ParamTypes, string ReturnType) {
        public bool Equals(MethodSignature other) {
            // signatures will be considered unique by (Name + ParamTypes)
            return Name == other.Name &&
                ParamTypes.SequenceEqual(other.ParamTypes);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            hashCode.Add(Name);

            foreach(string paramType in ParamTypes) {
                hashCode.Add(paramType);
            }

            return hashCode.ToHashCode();
        }
    }

    public class ClassType : TypeBase
    {
        ClassType ExtendingClassType { get; }
        public Constructor Constructor { get; }

        public HashSet<MethodSignature> MethodSignatures { get; } = new();

        // name to type
        public Dictionary<string, string> LocalClassFields { get; } = new();


        public ClassType(
            ClassDefinition classDefinition,
            ClassType extendingClassType
            ) 
            : this(classDefinition.Name.Value,
                    classDefinition.VariableDeclarations,
                    classDefinition.Constructor,
                    classDefinition.MethodDefinitions,
                    extendingClassType) {}


        public ClassType(
                string className,
                List<AST> variableDeclarations,
                AST constructor,
                List<AST> methodDefinitions,
                ClassType extendingClassType
                )
                : base(className)
        {
            ExtendingClassType = extendingClassType;

            InitializeLocalFields(variableDeclarations);

            Constructor = (Constructor)constructor;
            InitializeMethodSignatures(methodDefinitions);
        }
        
        public override bool CanBeAssignedTo(TypeBase other) {
            switch(other) {
                case ClassType classType: {
                    if(classType.Name == Name) return true;

                    if(ExtendingClassType is null) return false;

                    return ExtendingClassType.CanBeAssignedTo(classType);
                }

                default:
                  return false;
            }
        }

        public bool CanAccessField(string fieldName) {
            return LocalClassFields.ContainsKey(fieldName) ||
                   (
                    ExtendingClassType is not null &&
                    ExtendingClassType.CanAccessField(fieldName)
                   );
        }

        private void InitializeLocalFields(List<AST> variableDeclarations) {
            foreach(VariableDeclaration variableDeclaration in variableDeclarations) {
                if(CanAccessField(variableDeclaration.Var.Value)) {
                    throw new TypeException($"Redelcaration of field {variableDeclaration.Var.Value}");
                }

                LocalClassFields.Add(
                        variableDeclaration.Var.Value,
                        variableDeclaration.Type.Value
                        );
            }
        }

        private void InitializeMethodSignatures(List<AST> methodDefinitions) {
            foreach(MethodDefinition methodDefinition in methodDefinitions) {
                MethodSignature newMethodSignature = new MethodSignature(
                    methodDefinition.Name.Value,
                    methodDefinition.Parameters.Select(vd => ((VariableDeclaration)vd).Type.Value).ToList(),
                    methodDefinition.ReturnType.Value
                );

                if(MethodSignatures.Contains(newMethodSignature)) {
                    throw new TypeException($"Redeclaration of identical method {newMethodSignature.Name}");
                }

                MethodSignatures.Add(newMethodSignature);
            }
        }
    }


    public class TypeMap {
        private Dictionary<string, ClassDefinition> types = new();

        public TypeMap() : this([]) {}

        public TypeMap(IEnumerable<string> primitives) {
            foreach(string primitive in primitives) {
                types.Add(primitive, null);
            }
        }

        public void DefineType(ClassDefinition classDefinition) {
            AssertNotDefined(classDefinition.Name.Value);

            types.Add(classDefinition.Name.Value, classDefinition);
        }

        public bool CanBeAssignedTo(string assigningTypeName, string targetTypeName) {
            ClassDefinition targetType =  GetClassDefinition(targetTypeName);
            ClassDefinition assigningType = GetClassDefinition(assigningTypeName);

            // both primitives
            if(targetType is null && assigningType is null) return assigningTypeName == targetTypeName;

            // one primitive one not
            if(targetType is null || assigningType is null) return false;


            // both class types
            while(assigningType.Name.Value != targetTypeName) {
                if(assigningType.ExtendsName is null) {
                    return false;
                }

                assigningType = GetClassDefinition(assigningType.ExtendsName.Value);
            }

            return true;
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

        public ClassDefinition GetClassDefinition(string type) { 
            AssertDefined(type);
            return types[type];
        }
    }

    public class TypeChecker
    {
        private Scope scope = new(null);
        private Dictionary<string, TypeBase> Types = new();

        private static HashSet<string> Primitives = new([
                "Int",
                "Boolean",
                "Void"
        ]);

        private TypeMap TypeMap = new(Primitives);


        private ClassType CreateType(
            string className, 
            Dictionary<string, ClassDefinition> definedClasses,
            HashSet<string> workingTree) 
        {
            if(workingTree.Contains(className)) {
                // cyclic inheritance
                throw new TypeException($"Class {className} is part of an inheritance cycle");
            }

            if(Types.GetValueOrDefault(className, null) is ClassType classType) {
                // already defined this ClassType
                return classType;
            }

            if(!definedClasses.TryGetValue(className, out ClassDefinition classDefinition)) {
                throw new TypeException($"Class {className} is not defined");
            }


            ClassType extendingClassType = null;
            if(classDefinition.ExtendsName.Value is string extendsName) {
                if(!Types.ContainsKey(extendsName)) {
                    throw new TypeException($"Inherited class {extendsName} is not defined");
                }

                workingTree.Add(className);
                extendingClassType = CreateType(extendsName, definedClasses, workingTree);
            }

            return new(
                    classDefinition,
                    extendingClassType
                    );
        }

        private void ValidateClass(ClassType classType) {
            // ensures fields are valid types
            foreach((_, string typeName) in classType.LocalClassFields) {
                if(!Types.ContainsKey(typeName)) {
                    throw new TypeException($"Type {typeName} is not defined");
                }
            }


        }

        private void TypeCheckClasses(List<AST> classes) {
            Dictionary<string, ClassDefinition> definedClasses = new();

            // first pass: add classes to dictionary
            foreach(ClassDefinition classDefinition in classes) {
                if(definedClasses.ContainsKey(classDefinition.Name.Value)) {
                    throw new TypeException($"Class {classDefinition.Name.Value} defined more than once");
                }

                definedClasses.Add(classDefinition.Name.Value, classDefinition);
                Types.Add(classDefinition.Name.Value, null);
            }

            // second pass: convert ClassDefinitions into ClassTypes
            foreach(ClassDefinition classDefinition in classes) {
                HashSet<string> workingTree = new();
                Types.Add(classDefinition.Name.Value, CreateType(classDefinition.Name.Value, definedClasses, workingTree));
            }

            // third pass: validate class types :(
            foreach((_, TypeBase type) in Types) {
                if(type is ClassType classType) {
                    ValidateClass(classType);
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
                    TypeCheckClasses(prog.Classes);

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
