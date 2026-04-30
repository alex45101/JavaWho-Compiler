namespace JavaWhoCompiler
{
    public class CodeGeneratorException(string message) : Exception(message);

    public class CodeGenerator(StreamWriter streamWriter)
    {
        public static void Generate(ProgramNode programNode, string outPath)
        {
            StreamWriter streamWriter = new(outPath);

            CodeGenerator codeGenerator = new(streamWriter);
            foreach(ClassDefinition classDefinition in programNode.Classes)
            {
                codeGenerator.GenerateClass(classDefinition);
            }

            streamWriter.Close();
        }

        private void GenerateClass(ClassDefinition classDefinition)
        {
            streamWriter.Write($"class {classDefinition.Name.Value}");
            if (classDefinition.ExtendsName is (string extendsName, _))
            {
                streamWriter.Write($" extends {extendsName}");
            }

            streamWriter.WriteLine("{");

            foreach(AST variableDeclaration in classDefinition.VariableDeclarations)
            {
                GenerateStatement(variableDeclaration);
            }

            GenerateConstructor((Constructor)classDefinition.Constructor);

            foreach(MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
            {
                GenerateMethod(methodDefinition);
            }

            streamWriter.WriteLine("}");
        }

        private void GenerateConstructor(Constructor constructor)
        {
            streamWriter.Write("constructor(");
            GenerateCommaSeperated<VariableDeclaration>(constructor.Parameters, GenerateVariableDeclaration);
            streamWriter.Write(")");

            // constructor body start
            streamWriter.WriteLine("{");
            
            // super
            if (constructor.SuperArguments is List<AST> args)
            {
                streamWriter.Write("super(");
                GenerateCommaSeperated<AST>(args, GenerateExpression);
                streamWriter.WriteLine(");");
            }

            // statements
            foreach(AST statement in constructor.Statements)
            {
                GenerateStatement(statement);
            }

            streamWriter.WriteLine("}");
        }

        private void GenerateMethod(MethodDefinition methodDefinition)
        {
            streamWriter.Write($"{methodDefinition.Name.Value}");

            streamWriter.Write("(");
            GenerateCommaSeperated<VariableDeclaration>(methodDefinition.Parameters, GenerateVariableDeclaration);
            streamWriter.Write(")");
            
            GenerateBlockStatement((BlockStatement)methodDefinition.Body);
        }

        private void GenerateStatement(AST statement)
        {
            switch(statement)
            {
                case VariableDeclaration variableDeclaration:
                    GenerateVariableDeclaration(variableDeclaration);
                    streamWriter.WriteLine(";");
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
            streamWriter.WriteLine("{");
            foreach(AST statement in blockStatement.Statements)
            {
                GenerateStatement(statement);
            }
            streamWriter.WriteLine("}");
        }

        private void GenerateReturnStatement(ReturnStatement returnStatement)
        {
            streamWriter.Write("return");
            if(returnStatement.Val is AST expression)
            {
                GenerateExpression(expression);
            }

            streamWriter.WriteLine(";");
        }

        private void GenerateIdentifiedNode(IdentifiedNode identifiedNode)
        {
            if (identifiedNode.IsField)
            {
                streamWriter.Write("this.");
            }

            streamWriter.Write(identifiedNode.Value);
        }

        private void GenerateExpression(AST expression)
        {
            switch(expression)
            {
                case IdentifiedNode identifiedNode:
                    GenerateIdentifiedNode(identifiedNode);
                    break;
                case BooleanLiteral(bool value, _):
                    streamWriter.Write(value);
                    break;
                case IntLiteral(int value, _):
                    streamWriter.Write(value);
                    break;
                default:
                    throw new NotImplementedException();
                    // throw new CodeGeneratorException("Something is deeply wrong...");

            }
        }

        private void GenerateVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            // VariableDeclaration variableDeclaration = (VariableDeclaration)ast;

            GenerateIdentifiedNode(variableDeclaration.Var);
        }

        private void GenerateAssignmentStatement(AssignmentStatement assignmentStatement)
        {
            GenerateIdentifiedNode(assignmentStatement.Var);
            streamWriter.Write("=");
            GenerateExpression(assignmentStatement.Val);

            streamWriter.WriteLine(";");
        }

        private void GenerateCommaSeperated<T>(List<AST> items, Action<T> generate)
            where T: AST
        {
            for(int i = 0; i < items.Count - 1; i++)
            {
                generate((T)items[i]);
                streamWriter.Write(",");
            }

            generate((T)items[^1]);
        }
    }
}
