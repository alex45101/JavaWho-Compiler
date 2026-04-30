using System.ComponentModel;
using System.Text;
using JavaWhoCompiler;

namespace CompilerTests
{
    public class CodeGeneratorTests
    {
        public static IEnumerable<object[]> StatementCodeResults()
        {
            // vardec
            yield return new object[] {
                "Int x;",
                $"let x;{Environment.NewLine}"
            };

            // assign
            yield return new object[] {
                "Int x; x = 5;",
                $"""
                let x;
                x=5;

                """
            };
        }

        [Theory]
        [Trait("Category", "Statement")]
        [MemberData(nameof(StatementCodeResults))]
        public void GenerateStatementTest(string code, string expected)
        {
            var tokens = Tokenizer.Tokenize(code);
            var ast = Parser.Parse(tokens);

            StringBuilder stringBuilder = new();
            StringWriter stringWriter = new(stringBuilder);

            List<string> errors = TypeChecker.CheckType(ast);
            Assert.Empty(errors);

            ProgramNode programNode = (ProgramNode)ast;
            
            CodeGenerator codeGenerator = new(stringWriter);
            codeGenerator.GenerateProgram(programNode);

            stringWriter.Close();
            string result = stringBuilder.ToString();
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void TmpTest() {
            string code = """
                class Pet
                {
                    String Name;
                    Int Age;

                    init(String name, Int age)
                    {
                        Name = name;
                        Age = age;
                        Int x;
                        x = Age;
                    } 
                }

                class Cat extends Pet
                {
                    Int Lives;

                    init(String name, Int age, Int lives)
                    {
                        super(name, age);

                        Lives = lives;
                    }

                    method LoseLives(Int livesToLose) Boolean
                    {
                        Int tempLives;
                        tempLives = 0;

                        Lives = tempLives;

                        return true;
                    }
                }
                """;

                var tokens = Tokenizer.Tokenize(code);
                var ast = Parser.Parse(tokens);

                StringBuilder stringBuilder = new();
                StringWriter stringWriter = new(stringBuilder);

                List<string> errors = TypeChecker.CheckType(ast);
                Assert.Empty(errors);

                ProgramNode programNode = (ProgramNode)ast;
                
                CodeGenerator codeGenerator = new(stringWriter);
                codeGenerator.GenerateProgram(programNode);

                stringWriter.Close();
                
                string result = stringBuilder.ToString();
                Console.WriteLine(result);
        }
    }
}
