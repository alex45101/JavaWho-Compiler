using System.ComponentModel;
using System.Text;
using JavaWhoCompiler;
using Newtonsoft.Json.Bson;

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
        [Trait("Category", "CodeExamples")]
        public void SimpleClassExampleTest()
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
                    } 

                    method Info() Void
                    {
                        println("Pet Name: " + Name + ", Age: " + Age);
                    }
                }

                Pet john;
                john = new Pet("john", 8);

                john.Info();
                """;

            string expected = """
                class Pet {
                    Name;
                    Age;
                    constructor(name, age) {
                        this.Name = name;
                        this.Age = age;
                    }
                    Empty_Info_Void() {
                        console.log("Pet Name: " + this.Name + ", Age: " + this.Age);
                    }
                }
                let john;
                john = new Pet("john", 8);
                john.Empty_Info_Void();
                
                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "CodeExamples")]
        public void InheritanceClassExampleTest()
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
                    } 

                    method Info() Void
                    {
                        println("Pet Name: " + Name + ", Age: " + Age);
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

                    method Lives() Void
                    {
                        println("Lives: " + Lives);
                    }

                    method LoseLives(Int livesToLose) Boolean
                    {
                        Int tempLives;
                        tempLives = Lives - livesToLose;

                        if (tempLives < 0)
                        {
                            return false;
                        }

                        Lives = tempLives;

                        return true;
                    }
                }

                Pet john;
                john = new Pet("john", 8);

                john.Info();

                Cat sam;
                sam = new Cat("sam", 1, 9);

                sam.Info();
                sam.Lives();

                Boolean result;
                result = sam.LoseLives(1);
                println(result);
                sam.Lives();

                result = sam.LoseLives(67);
                println(result);
                sam.Lives();
                """;

            string expected = """
                class Pet {
                    Name;
                    Age;
                    constructor(name, age) {
                        this.Name = name;
                        this.Age = age;
                    }
                    Empty_Info_Void() {
                        console.log("Pet Name: " + this.Name + ", Age: " + this.Age);
                    }
                }
                class Cat extends Pet {
                    Lives;
                    constructor(name, age, lives) {
                        super(name, age);
                        this.Lives = lives;
                    }
                    Empty_Lives_Void() {
                        console.log("Lives: " + this.Lives);
                    }
                    Int_LoseLives_Boolean(livesToLose) {
                        let tempLives;
                        tempLives = this.Lives - livesToLose;
                        if(tempLives < 0) {
                            return false;
                        }
                        this.Lives = tempLives;
                        return true;
                    }
                }
                let john;
                john = new Pet("john", 8);
                john.Empty_Info_Void();
                let sam;
                sam = new Cat("sam", 1, 9);
                sam.Empty_Info_Void();
                sam.Empty_Lives_Void();
                let result;
                result = sam.Int_LoseLives_Boolean(1);
                console.log(result);
                sam.Empty_Lives_Void();
                result = sam.Int_LoseLives_Boolean(67);
                console.log(result);
                sam.Empty_Lives_Void();
                
                """;

            AssertHelperExpectedResultCode(expected, code);
        }

        [Fact]
        [Trait("Category", "CodeExamples")]
        public void LoopExamplesTest()
        { 
            string code = """
                println("Normal while loop counter:");

                Int a;
                a = 0;

                while(a < 5)
                {
                    println(a);
                    a = a + 1;
                }

                println("Do while loop counter:");

                Int b;
                b = 0;

                while(true)
                {
                    println(b);

                    b = b + 1;

                    if(b < 4)
                    {
                        break;
                    }
                }

                println("String loop counter with a's:");

                String text;
                text = "";

                while (text != "aaaa")
                {
                    if(text == "aa")
                    {
                        println("double aa, yaya");
                    }

                    println(text);
                    text = text + "a";
                }
                """;

            string expected = """
                console.log("Normal while loop counter:");
                let a;
                a = 0;
                while(a < 5) {
                    console.log(a);
                    a = a + 1;
                }
                console.log("Do while loop counter:");
                let b;
                b = 0;
                while(true) {
                    console.log(b);
                    b = b + 1;
                    if(b < 4) {
                        break;
                    }
                }
                console.log("String loop counter with a's:");
                let text;
                text = "";
                while(text != "aaaa") {
                    if(text == "aa") {
                        console.log("double aa, yaya");
                    }
                    console.log(text);
                    text = text + "a";
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
