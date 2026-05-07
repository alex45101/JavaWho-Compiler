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
                $"""
                let x;

                """
            };

            // assign
            yield return new object[] {
                "Int x; x = 5;",
                $"""
                let x;
                x = 5;

                """
            };

            // block
            yield return new object[] {
                "{ Int x; x = 5; }",
                $$"""
                {
                    let x;
                    x = 5;
                }

                """
            };
        }

        private void AssertHelperExpectedResultCode(string expected, string code)
        {
            var tokens = Tokenizer.Tokenize(code);
            var ast = Parser.Parse(tokens);

            StringBuilder stringBuilder = new();
            StringWriter stringWriter = new(stringBuilder);

            List<string> errors = TypeChecker.CheckType(ast);
            Assert.Empty(errors);

            ProgramNode programNode = (ProgramNode)ast;

            string result = CodeGenerator.Generate(programNode);

            Assert.Equal(expected, result);
        }

        [Theory]
        [Trait("Category", "Statement")]
        [MemberData(nameof(StatementCodeResults))]
        public void GenerateStatementTest(string code, string expected)
        {
            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void GenerateEmptyClassTest()
        {
            string code = """
                class Test {
                    init() {}
                }
                """;
            string expected = """
                class Test {
                    constructor() {
                    }
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void GenerateExtendsClassTest()
        {
            string code = """
                class Test extends Object {
                    init() {}
                }
                """;
            string expected = """
                class Test extends Object {
                    constructor() {
                    }
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void GenerateClassWithSuperTest()
        {
            string code = """
                class ExtString extends String {
                    init(String arg) {
                        super(arg);
                        Int x;
                    }
                }
                """;
            string expected = """
                class ExtString extends String {
                    constructor(arg) {
                        super(arg);
                        let x;
                    }
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void PrintlnExpression()
        {
            string code = """
                println(4);
                """;

            string expected = """
                console.log(4);

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void BinaryIntAddExpression()
        {
            string code = """
                Int x;

                x = 4 + 5;
                """;

            string expected = """
                let x;
                x = 4 + 5;

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void BinaryIntSubtractExpression()
        {
            string code = """
                Int x;

                x = 4 - 5;
                """;

            string expected = """
                let x;
                x = 4 - 5;

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void BinaryIntMultiplyExpression()
        {
            string code = """
                Int x;

                x = 4 * 5;
                """;

            string expected = """
                let x;
                x = 4 * 5;

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void BinaryIntDivideExpression()
        {
            string code = """
                Int x;

                x = 4 / 5;
                """;

            string expected = """
                let x;
                x = 4 / 5;

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "IfStatments")]
        public void IfStatementTest()
        {
            string code = """
                Int x;

                x = 4;

                if(x < 5)
                {
                    println("Well hello, there");
                }
                """;

            string expected = """
                let x;
                x = 4;
                if(x < 5) {
                    console.log("Well hello, there");
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "IfStatments")]

        public void IfElseStatementTest()
        {
            string code = """
                Int x;

                x = 4;

                if(x < 5)
                {
                    println("Well hello, there");
                }
                else
                {
                    println("No hello, there");
                }
                """;

            string expected = """
                let x;
                x = 4;
                if(x < 5) {
                    console.log("Well hello, there");
                }
                else {
                    console.log("No hello, there");
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "IfStatments")]
        public void IfElseIfElseStatementTest()
        {
            string code = """
                Int x;

                x = 4;

                if(x < 5)
                {
                    println("Well hello, there");
                }
                else if (x < 3)
                {
                    println("No hello, there");
                }
                else
                {
                    println("ooops");
                }
                """;

            string expected = """
                let x;
                x = 4;
                if(x < 5) {
                    console.log("Well hello, there");
                }
                else if(x < 3) {
                    console.log("No hello, there");
                }
                else {
                    console.log("ooops");
                }

                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "IfStatments")]
        public void IfElseNestedTest()
        {
            string code = """
                Int x;

                x = 4;

                if(x < 5)
                {
                    Int y;
                    y = 3;

                    if(y < 4)
                    {
                        println("Well hello, there");
                    }
                    else
                    {
                        if(x < 4)
                        {
                            println("Thing");
                        }
                        println("Nope");
                    }
                    
                }
                else if (x < 3)
                {
                    if(x == 3)
                    {
                        println("swag");
                    }

                    println("No hello, there");
                }
                else
                {
                    if(x == 5)
                    {
                        println("cool");
                    }
                    else if (x == 4)
                    {
                        println("sup");
                    }
                    println("ooops");
                }
                """;

            string expected = """
                let x;
                x = 4;
                if(x < 5) {
                    let y;
                    y = 3;
                    if(y < 4) {
                        console.log("Well hello, there");
                    }
                    else {
                        if(x < 4) {
                            console.log("Thing");
                        }
                        console.log("Nope");
                    }
                }
                else if(x < 3) {
                    if(x == 3) {
                        console.log("swag");
                    }
                    console.log("No hello, there");
                }
                else {
                    if(x == 5) {
                        console.log("cool");
                    }
                    else if(x == 4) {
                        console.log("sup");
                    }
                    console.log("ooops");
                }
                
                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        public void TmpTest()
        {
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
