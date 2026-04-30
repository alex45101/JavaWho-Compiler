namespace JavaWhoCompiler
{
    public class CodeGeneratorException(string message) : Exception(message);

    public class CodeGenerator(TextWriter textWriter)
    {
        private int targetIndent = 0;
        private int curIndent = 0;

        private void Indent() => targetIndent += 1;
        private void Dedent() => targetIndent -= 1;

        public readonly static string INDENT_TEXT = "    ";

        public static void Generate(ProgramNode programNode, string outPath)
        {
            StreamWriter streamWriter = new(outPath);

            CodeGenerator codeGenerator = new(streamWriter);
            codeGenerator.GenerateProgram(programNode);

            streamWriter.Close();
        }

        public void GenerateProgram(ProgramNode programNode)
        {
            foreach(ClassDefinition classDefinition in programNode.Classes)
            {
                GenerateClass(classDefinition);
            }

            foreach(AST statement in programNode.Statements)
            {
                GenerateStatement(statement);
            }
        }

        private void GenerateClass(ClassDefinition classDefinition)
        {
            Write($"class {classDefinition.Name.Value}");
            if (classDefinition.ExtendsName is (string extendsName, _))
            {
                Write($" extends {extendsName}");
            }

            WriteLine(" {");
            Indent();

            foreach(VariableDeclaration variableDeclaration in classDefinition.VariableDeclarations)
            {
                GenerateField(variableDeclaration);
            }

            GenerateConstructor((Constructor)classDefinition.Constructor);

            foreach(MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
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
            foreach(AST statement in constructor.Statements)
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
            switch(statement)
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
                default:
                    throw new NotImplementedException();
                    // throw new CodeGeneratorException("Something is deeply wrong...");
            }
        }

        private void GenerateBlockStatement(BlockStatement blockStatement)
        {
            WriteLine("{");
            Indent();
            foreach(AST statement in blockStatement.Statements)
            {
                GenerateStatement(statement);
            }
            Dedent();
            WriteLine("}");
        }

        private void GenerateReturnStatement(ReturnStatement returnStatement)
        {
            Write("return");
            if(returnStatement.Val is AST expression)
            {
                Write(" ");
                GenerateExpression(expression);
            }

            WriteLine(";");
        }

        private void GenerateIdentifiedNode(IdentifiedNode identifiedNode)
        {
            if (identifiedNode.IsField)
            {
                Write("this.");
            }

            Write(identifiedNode.Value);
        }

        private void GenerateExpression(AST expression)
        {
            switch(expression)
            {
                case IdentifiedNode identifiedNode:
                    GenerateIdentifiedNode(identifiedNode);
                    break;
                case BooleanLiteral(bool value, _):
                    Write(value.ToString().ToLower());
                    break;
                case IntLiteral(int value, _):
                    Write(value.ToString());
                    break;
                default:
                    throw new NotImplementedException();
                    // throw new CodeGeneratorException("Something is deeply wrong...");

            }
        }

        private void GenerateField(VariableDeclaration fieldVariableDeclaration)
        {
            WriteLine($"{fieldVariableDeclaration.Var.Value};");
        }

        private void GenerateParameter(VariableDeclaration fieldVariableDeclaration)
        {
            Write(fieldVariableDeclaration.Var.Value);
        }

        private void GenerateVariableDeclarationStatement(VariableDeclaration variableDeclaration)
        {
            WriteLine($"let {variableDeclaration.Var.Value};");
        }

        private void GenerateAssignmentStatement(AssignmentStatement assignmentStatement)
        {
            GenerateIdentifiedNode(assignmentStatement.Var);
            Write(" = ");
            GenerateExpression(assignmentStatement.Val);

            WriteLine(";");
        }

        private void GenerateCommaSeperated<T>(List<AST> items, Action<T> generate)
            where T: AST
        {
            for(int i = 0; i < items.Count - 1; i++)
            {
                generate((T)items[i]);
                Write(", ");
            }

            generate((T)items[^1]);
        }

        private void Write(string text)
        {
            while(curIndent < targetIndent)
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
