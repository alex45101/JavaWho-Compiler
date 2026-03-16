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

    public class MethodSignature {

    }

    public class ClassType(
        string className,
        ClassType extendingClassType,
        List<VariableDeclaration> variableDeclarations,
        Constructor constructor,
        List<MethodSignature> methodSignatures
        ) {
        string ClassName { get; } = className;
        ClassType ExtendingClassType { get; } = extendingClassType;
        List<VariableDeclaration> VariableDeclarations { get; } = variableDeclarations;
        Constructor Constructor { get; } = constructor;
        List<MethodSignature> MethodSignatures { get; } = methodSignatures;
        
        public bool IsSubtypeOf(ClassType classType) {
            if(classType.ClassName == ClassName) return true;

            if(ExtendingClassType is null) return false;

            return ExtendingClassType.IsSubtypeOf(classType);
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
                                                   Dictionary<string, ClassDefinition> definedClasses) 
        {
            if(ClassTypes.GetValueOrDefault(className, null) is ClassType classType) {
                // already defined this ClassType
                return classType;
            }

            ClassDefinition classDefinition = null;
            if(!definedClasses.TryGetValue(className, out classDefinition)) {
                throw new TypeException($"Class {className} is not defined");
            }


            ClassType extendingClassType = null;
            if(classDefinition.ExtendsName.Value is string extendsName) {
                if(!ClassTypes.ContainsKey(extendsName)) {
                    throw new TypeException($"Inherited class {extendsName} is not defined");
                }

                extendingClassType = ConvertAndCheckClassType(extendsName, definedClasses);
            }


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
                if(ClassTypes.GetValueOrDefault(classdef.Name.Value, null) is not null) {
                     // already defined
                     continue;
                }

                ClassTypes.Add(classdef.Name.Value, ConvertAndCheckClassType(classdef.Name.Value, DefinedClasses));

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
