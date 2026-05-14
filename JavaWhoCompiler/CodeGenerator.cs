using System.Text;

namespace JavaWhoCompiler
{
    public class CodeGeneratorException(string message) : Exception(message);

    public class CodeGenerator(TextWriter textWriter)
    {
        private int targetIndent = 0;
        private int curIndent = 0;

        private readonly HashSet<string> BuiltInClassNames = [..TypeBase.BuiltIns.Select(bi => bi.Name)];

        private void Indent() => targetIndent += 1;
        private void Dedent() => targetIndent -= 1;

        private readonly static string INDENT_TEXT = "    ";

        public static string Generate(ProgramNode programNode)
        {
            StringBuilder builder = new();
            StringWriter writer = new(builder);

            CodeGenerator codeGenerator = new(writer);
            codeGenerator.GenerateProgram(programNode);

            writer.Close();

            return builder.ToString();
        }

        public static void Generate(ProgramNode programNode, string outPath)
        {
            StreamWriter streamWriter = new(outPath);

            CodeGenerator codeGenerator = new(streamWriter);
            codeGenerator.GenerateProgram(programNode);

            streamWriter.Close();
        }

        public void GenerateProgram(ProgramNode programNode)
        {
            foreach (ClassDefinition classDefinition in programNode.Classes)
            {
                GenerateClass(classDefinition);
            }

            foreach (AST statement in programNode.Statements)
            {
                GenerateStatement(statement);
            }
        }

        private void GenerateClass(ClassDefinition classDefinition)
        {
            Write($"class ");
            GenerateClassName(classDefinition.Name.Value);
            if (classDefinition.ExtendsName is (string extendsName, _))
            {
                Write($" extends ");
                GenerateClassName(extendsName);
            }

            WriteLine(" {");
            Indent();

            foreach (VariableDeclaration variableDeclaration in classDefinition.VariableDeclarations)
            {
                GenerateField(variableDeclaration);
            }

            GenerateConstructor((Constructor)classDefinition.Constructor);

            foreach (MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
            {
                GenerateMethod(methodDefinition);
            }

            Dedent();
            WriteLine("}");
        }

        private void GenerateConstructor(Constructor constructor)
        {
            Write("constructor(");
            GenerateCommaSeperated<VariableDeclaration>(constructor.Parameters, GenerateParameter);
            Write(")");

            // constructor body start
            WriteLine(" {");
            Indent();

            // super
            if (constructor.SuperArguments is List<AST> args)
            {
                Write("super(");
                GenerateCommaSeperated<AST>(args, GenerateExpression);
                WriteLine(");");
            }

            // statements
            foreach (AST statement in constructor.Statements)
            {
                GenerateStatement(statement);
            }

            Dedent();
            WriteLine("}");
        }

        private void GenerateMethod(MethodDefinition methodDefinition)
        {
            Write($"{methodDefinition.AnnotatedMethodName}");

            Write("(");
            GenerateCommaSeperated<VariableDeclaration>(methodDefinition.Parameters, GenerateParameter);
            Write(") ");

            GenerateBlockStatement((BlockStatement)methodDefinition.Body);
        }

        private void GenerateStatement(AST statement)
        {
            switch (statement)
            {
                case VariableDeclaration variableDeclaration:
                    GenerateVariableDeclarationStatement(variableDeclaration);
                    break;
                case AssignmentStatement assignmentStatement:
                    GenerateAssignmentStatement(assignmentStatement);
                    break;
                case BlockStatement blockStatement:
                    GenerateBlockStatement(blockStatement);
                    break;
                case ReturnStatement returnStatement:
                    GenerateReturnStatement(returnStatement);
                    break;
                case ExpressionStatement expressionStatement:
                    GenerateExpression(expressionStatement.Expression);
                    WriteLine(";");
                    break;
                case WhileStatement whileStatement:
                    GenerateWhileStatement(whileStatement);
                    break;
                case IfStatement ifStatement:
                    GenerateIfStatement(ifStatement);
                    break;
                case BreakStatement:
                    WriteLine("break;");
                    break;
                default:
                    throw new CodeGeneratorException("Something is deeply wrong...");
            }
        }

        private void GenerateIfStatement(IfStatement ifStatement)
        {
            GenerateConditional("if", ifStatement.Guard, ifStatement.IfBody);

            AST currentElse = ifStatement.ElseBody;

            while (currentElse is not null)
            {
                switch (currentElse)
                {
                    case IfStatement currIfStatement:                        
                        GenerateConditional("else if", currIfStatement.Guard, currIfStatement.IfBody);
                        currentElse = currIfStatement.ElseBody;
                        break;
                    case AST:
                        GenerateConditional("else", null, currentElse);
                        currentElse = null;
                        break;
                    default:
                        throw new CodeGeneratorException("Something went wrong...");

                }
            }
        }

        private void GenerateWhileStatement(WhileStatement whileStatement)
        {
            GenerateConditional("while", whileStatement.Guard, whileStatement.Statement);
        }

        private void GenerateConditional(string conditionalName, AST guard, AST body)
        {
            Write(conditionalName);

            if (guard is not null)
            {
                Write("(");
                GenerateExpression(guard);
                Write(")");
            }

            if (body is BlockStatement blockStatement)
            {
                Write(" ");
                GenerateBlockStatement(blockStatement);
            }
            else
            {
                WriteLine("");
                Indent();
                GenerateStatement(body);
                Dedent();
            }
        }

        private void GenerateBlockStatement(BlockStatement blockStatement)
        {
            WriteLine("{");
            Indent();
            foreach (AST statement in blockStatement.Statements)
            {
                GenerateStatement(statement);
            }
            Dedent();
            WriteLine("}");
        }

        private void GenerateReturnStatement(ReturnStatement returnStatement)
        {
            Write("return");
            if (returnStatement.Val is AST expression)
            {
                Write(" ");
                GenerateExpression(expression);
            }

            WriteLine(";");
        }

        private void GeneratePrefixed(string value)
        {
            // _ prefix to avoid collision with annotated method names or js keywords
            Write($"_{value}");
        }

        private void GenerateVariable(string varName) => GeneratePrefixed(varName);

        private void GenerateClassName(string className) {
            if (BuiltInClassNames.Contains(className))
            {
                Write(className);
            }
            else
            {
                GeneratePrefixed(className);
            }
        }

        private void GenerateVariableExpression(IdentifiedNode identifiedNode)
        {
            if (identifiedNode.IsField)
            {
                Write("this.");
            }

            GenerateVariable(identifiedNode.Value);
        }

        private void GenerateExpression(AST expression)
        {
            switch (expression)
            {
                case IdentifiedNode identifiedNode:
                    GenerateVariableExpression(identifiedNode);
                    break;
                case BooleanLiteral(bool value, _):
                    Write(value.ToString().ToLower());
                    break;
                case IntLiteral(int value, _):
                    Write(value.ToString());
                    break;
                case StringLiteral(string value, _):
                    Write(value);
                    break;
                case ThisExpression:
                    Write("this");
                    break;
                case PrintLnStatement printLnStatement:
                    Write($"console.log(");

                    // write (expression.toString()) to prevent String class objects from printing differently
                    Write("(");
                    GenerateExpression(printLnStatement.Argument);
                    Write(").toString()");

                    Write(")");
                    break;
                case MethodCallExpression methodCallExpression:
                    GenerateExpression(methodCallExpression.Target);
                    Write($".{methodCallExpression.AnnotatedMethodName}(");
                    GenerateCommaSeperated<AST>(methodCallExpression.Arguments, GenerateExpression);
                    Write(")");
                    break;
                case BinaryExpression(AST left, OperatorType operatorType, AST right, _):
                    GenerateExpression(left);
                    Write(" ");
                    GenerateOperator(operatorType);
                    Write(" ");
                    GenerateExpression(right);
                    break;
                case NewObjectExpression newObjectExpression:
                    GenerateNewObjectExpression(newObjectExpression);
                    break;
                default:
                    throw new CodeGeneratorException("Something is deeply wrong...");

            }
        }

        private void GenerateNewObjectExpression(NewObjectExpression newObjectExpression)
        {
            Write($"new ");
            GenerateClassName(newObjectExpression.ClassName.Value);
            Write("(");
            GenerateCommaSeperated<AST>(newObjectExpression.Arguments, GenerateExpression);
            Write(")");
        }

        private void GenerateOperator(OperatorType operatorType)
        {
            string operatorString = operatorType switch
            {
                OperatorType.LessThan => "<",
                OperatorType.Add => "+",
                OperatorType.Subtract => "-",
                OperatorType.Multiply => "*",
                OperatorType.Divide => "/",
                OperatorType.Equal => "==",
                OperatorType.NotEqual => "!=",
                _ => throw new CodeGeneratorException("Something is deeply wrong..."),
            };

            Write(operatorString);
        }

        private void GenerateField(VariableDeclaration fieldVariableDeclaration)
        {
            GenerateVariable(fieldVariableDeclaration.Var.Value);
            WriteLine(";");
        }

        private void GenerateParameter(VariableDeclaration fieldVariableDeclaration)
        {
            GenerateVariable(fieldVariableDeclaration.Var.Value);
        }

        private void GenerateVariableDeclarationStatement(VariableDeclaration variableDeclaration)
        {
            Write("let ");
            GenerateVariable(variableDeclaration.Var.Value);
            WriteLine(";");
        }

        private void GenerateAssignmentStatement(AssignmentStatement assignmentStatement)
        {
            GenerateVariableExpression(assignmentStatement.Var);
            Write(" = ");
            GenerateExpression(assignmentStatement.Val);

            WriteLine(";");
        }

        private void GenerateCommaSeperated<T>(List<AST> items, Action<T> generate)
            where T : AST
        {
            if (items.Count == 0)
            {
                return;
            }

            for (int i = 0; i < items.Count - 1; i++)
            {
                generate((T)items[i]);
                Write(", ");
            }

            generate((T)items[^1]);
        }

        private void Write(string text)
        {
            while (curIndent < targetIndent)
            {
                textWriter.Write(INDENT_TEXT);
                curIndent += 1;
            }

            textWriter.Write(text);
        }

        private void WriteLine(string text)
        {
            Write(text);
            textWriter.WriteLine();
            curIndent = 0;
        }
    }
}
