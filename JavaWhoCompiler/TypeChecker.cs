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
        public abstract bool IsSubtypeOf(TypeBase other);


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
        public override bool IsSubtypeOf(TypeBase other) {
            return Equals(other);
        }
    }


    public class TypeList(IEnumerable<TypeBase> types) : List<TypeBase>(types) {

        public static TypeList FromASTNodes(List<AST> variableDeclarations) {
            return new TypeList(variableDeclarations.Select(vd => {
                            throw new NotImplementedException();
                            return TypeBase.IntPrimitive;
                        }));
        }

        public override bool Equals(Object other) {
            return other is TypeList paramTypes &&
                    this.SequenceEqual(paramTypes);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            foreach(TypeBase paramType in this) {
                hashCode.Add(paramType.GetHashCode());
            }

            return hashCode.ToHashCode();
        }
    }

    public sealed record MethodSignature(string Name, TypeList Parameters, string ReturnType);

    public class ClassType : TypeBase
    {
        ClassType ExtendingClassType { get; }
        Constructor Constructor { get; }

        HashSet<MethodSignature> MethodSignatures { get; } = new();

        // name to type
        Dictionary<string, string> LocalClassFields { get; } = new();


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
        
        public override bool IsSubtypeOf(TypeBase other) {
            switch(other) {
                case ClassType classType: {
                    if(classType.Name == Name) return true;

                    if(ExtendingClassType is null) return false;

                    return ExtendingClassType.IsSubtypeOf(classType);
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
            foreach(VariableDeclaration vd in variableDeclarations) {
                if(CanAccessField(vd.Var.Value)) {
                    throw new TypeException($"Redelcaration of field {vd.Var.Value}");
                }

                LocalClassFields.Add(
                        vd.Var.Value,
                        vd.Type.Value
                        );
            }
        }

        private void InitializeMethodSignatures(List<AST> methodDefinitions) {
            foreach(MethodDefinition md in methodDefinitions) {
                MethodSignatures.Add(
                        new MethodSignature(
                            md.Name.Value,
                            TypeList.FromASTNodes(md.Parameters),
                            // md.Parameters.Select(vd => ((VariableDeclaration)vd).Type.Value).ToList(),
                            md.ReturnType.Value
                            )
                        );
            }
        }
    }

    public class TypeChecker
    {
        private Scope scope = new(null);
        private Dictionary<string, ClassType> ClassTypes = new();

        public static void CheckType(AST node)
        { 
            TypeChecker typeChecker = new TypeChecker();

            typeChecker.CheckTypeHelper(node);
        }


        private void CheckClassType(ClassType classType) {
            throw new NotImplementedException();
        }

        private ClassType ConvertAndCheckClassType(string className, 
                                                   Dictionary<string, ClassDefinition> definedClasses,
                                                   HashSet<string> workingTree) 
        {
            if(workingTree.Contains(className)) {
                // cyclic inheritance
                throw new TypeException($"Class {className} is part of an inheritance cycle");
            }

            if(ClassTypes.GetValueOrDefault(className, null) is ClassType classType) {
                // already defined this ClassType
                return classType;
            }

            if(!definedClasses.TryGetValue(className, out ClassDefinition classDefinition)) {
                throw new TypeException($"Class {className} is not defined");
            }


            ClassType extendingClassType = null;
            if(classDefinition.ExtendsName.Value is string extendsName) {
                if(!ClassTypes.ContainsKey(extendsName)) {
                    throw new TypeException($"Inherited class {extendsName} is not defined");
                }

                workingTree.Add(className);
                extendingClassType = ConvertAndCheckClassType(extendsName, definedClasses, workingTree);
            }

            ClassType currentClassType = new(
                    className,
                    extendingClassType,
                    classDefinition.VariableDeclarations,
                    classDefinition.Constructor,

                    )
            
            
            
            return null;
        }

        private void TypeCheckClasses(List<AST> classes) {
            Dictionary<string, ClassDefinition> DefinedClasses = new();

            // first pass: add classes to dictionary
            foreach(ClassDefinition classdef in classes) {
                if(DefinedClasses.ContainsKey(classdef.Name.Value)) {
                    throw new TypeException($"Class {classdef.Name.Value} defined more than once");
                }

                DefinedClasses.Add(classdef.Name.Value, classdef);
                ClassTypes.Add(classdef.Name.Value, null);
            }

            // second pass: convert and check validity of class
            foreach(ClassDefinition classdef in classes) {
                HashSet<string> workingTree = new();
                ClassTypes.Add(classdef.Name.Value, ConvertAndCheckClassType(classdef.Name.Value, DefinedClasses, workingTree));
            }
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
