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

    public class TypeChecker
    {
        private Scope scope = new(null);

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
                    //todo add classes

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
