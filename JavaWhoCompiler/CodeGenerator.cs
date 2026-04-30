namespace JavaWhoCompiler
{
    public class CodeGeneratorException(string message) : Exception(message);

    public class CodeGenerator(TextWriter textWriter)
    {
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
            textWriter.Write($"class {classDefinition.Name.Value}");
            if (classDefinition.ExtendsName is (string extendsName, _))
            {
                textWriter.Write($" extends {extendsName}");
            }

            textWriter.WriteLine("{");

            foreach(VariableDeclaration variableDeclaration in classDefinition.VariableDeclarations)
            {
                GenerateField(variableDeclaration);
            }

            GenerateConstructor((Constructor)classDefinition.Constructor);

            foreach(MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
            {
                GenerateMethod(methodDefinition);
            }

            textWriter.WriteLine("}");
        }

        private void GenerateConstructor(Constructor constructor)
        {
            textWriter.Write("constructor(");
            GenerateCommaSeperated<VariableDeclaration>(constructor.Parameters, GenerateParameter);
            textWriter.Write(")");

            // constructor body start
            textWriter.WriteLine("{");
            
            // super
            if (constructor.SuperArguments is List<AST> args)
            {
                textWriter.Write("super(");
                GenerateCommaSeperated<AST>(args, GenerateExpression);
                textWriter.WriteLine(");");
            }

            // statements
            foreach(AST statement in constructor.Statements)
            {
                GenerateStatement(statement);
            }

            textWriter.WriteLine("}");
        }

        private void GenerateMethod(MethodDefinition methodDefinition)
        {
            textWriter.Write($"{methodDefinition.Name.Value}");

            textWriter.Write("(");
            GenerateCommaSeperated<VariableDeclaration>(methodDefinition.Parameters, GenerateParameter);
            textWriter.Write(")");
            
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
            textWriter.WriteLine("{");
            foreach(AST statement in blockStatement.Statements)
            {
                GenerateStatement(statement);
            }
            textWriter.WriteLine("}");
        }

        private void GenerateReturnStatement(ReturnStatement returnStatement)
        {
            textWriter.Write("return");
            if(returnStatement.Val is AST expression)
            {
                textWriter.Write(" ");
                GenerateExpression(expression);
            }

            textWriter.WriteLine(";");
        }

        private void GenerateIdentifiedNode(IdentifiedNode identifiedNode)
        {
            if (identifiedNode.IsField)
            {
                textWriter.Write("this.");
            }

            textWriter.Write(identifiedNode.Value);
        }

        private void GenerateExpression(AST expression)
        {
            switch(expression)
            {
                case IdentifiedNode identifiedNode:
                    GenerateIdentifiedNode(identifiedNode);
                    break;
                case BooleanLiteral(bool value, _):
                    textWriter.Write(value.ToString().ToLower());
                    break;
                case IntLiteral(int value, _):
                    textWriter.Write(value);
                    break;
                default:
                    throw new NotImplementedException();
                    // throw new CodeGeneratorException("Something is deeply wrong...");

            }
        }

        private void GenerateField(VariableDeclaration fieldVariableDeclaration)
        {
            textWriter.WriteLine($"{fieldVariableDeclaration.Var.Value};");
        }

        private void GenerateParameter(VariableDeclaration fieldVariableDeclaration)
        {
            textWriter.Write(fieldVariableDeclaration.Var.Value);
        }

        private void GenerateVariableDeclarationStatement(VariableDeclaration variableDeclaration)
        {
            textWriter.WriteLine($"let {variableDeclaration.Var.Value};");
        }

        private void GenerateAssignmentStatement(AssignmentStatement assignmentStatement)
        {
            GenerateIdentifiedNode(assignmentStatement.Var);
            textWriter.Write("=");
            GenerateExpression(assignmentStatement.Val);

            textWriter.WriteLine(";");
        }

        private void GenerateCommaSeperated<T>(List<AST> items, Action<T> generate)
            where T: AST
        {
            for(int i = 0; i < items.Count - 1; i++)
            {
                generate((T)items[i]);
                textWriter.Write(",");
            }

            generate((T)items[^1]);
        }
    }
}
