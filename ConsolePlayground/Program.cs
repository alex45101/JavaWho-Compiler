using JavaWhoCompiler;

namespace ConsolePlayground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Expected 2 arguments. [SourceFile] [OutPath]");
                    Environment.Exit(1);
                }

                string sourceFile = args[0];
                string outPath = args[1];

                string code = File.ReadAllText(sourceFile);

                IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);

                ProgramNode programNode = (ProgramNode)Parser.Parse(tokens);

                List<string> errors = TypeChecker.CheckType(programNode);
                if (errors.Count > 0)
                {
                    foreach(string error in errors)
                    {
                        Console.Error.WriteLine(error);
                    }
                    Environment.Exit(1);
                }

                CodeGenerator.Generate(programNode, outPath);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}
