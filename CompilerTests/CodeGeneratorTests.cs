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
                let _x;

                """
            };

            // assign
            yield return new object[] {
                "Int x; x = 5;",
                $"""
                let _x;
                _x = 5;

                """
            };

            // block
            yield return new object[] {
                "{ Int x; x = 5; }",
                $$"""
                {
                    let _x;
                    _x = 5;
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
                class _Test {
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
                class _Test extends Object {
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
                class _ExtString extends String {
                    constructor(_arg) {
                        super(_arg);
                        let _x;
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
                let _x;
                _x = 4 + 5;

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
                let _x;
                _x = 4 - 5;

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
                let _x;
                _x = 4 * 5;

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
                let _x;
                _x = 4 / 5;

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
                let _x;
                _x = 4;
                if(_x < 5) {
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
                let _x;
                _x = 4;
                if(_x < 5) {
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
                let _x;
                _x = 4;
                if(_x < 5) {
                    console.log("Well hello, there");
                }
                else if(_x < 3) {
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
                let _x;
                _x = 4;
                if(_x < 5) {
                    let _y;
                    _y = 3;
                    if(_y < 4) {
                        console.log("Well hello, there");
                    }
                    else {
                        if(_x < 4) {
                            console.log("Thing");
                        }
                        console.log("Nope");
                    }
                }
                else if(_x < 3) {
                    if(_x == 3) {
                        console.log("swag");
                    }
                    console.log("No hello, there");
                }
                else {
                    if(_x == 5) {
                        console.log("cool");
                    }
                    else if(_x == 4) {
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
                class _Pet {
                    _Name;
                    _Age;
                    constructor(_name, _age) {
                        this._Name = _name;
                        this._Age = _age;
                    }
                    Empty_Info_Void() {
                        console.log("Pet Name: " + this._Name + ", Age: " + this._Age);
                    }
                }
                let _john;
                _john = new _Pet("john", 8);
                _john.Empty_Info_Void();
                
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
                class _Pet {
                    _Name;
                    _Age;
                    constructor(_name, _age) {
                        this._Name = _name;
                        this._Age = _age;
                    }
                    Empty_Info_Void() {
                        console.log("Pet Name: " + this._Name + ", Age: " + this._Age);
                    }
                }
                class _Cat extends _Pet {
                    _Lives;
                    constructor(_name, _age, _lives) {
                        super(_name, _age);
                        this._Lives = _lives;
                    }
                    Empty_Lives_Void() {
                        console.log("Lives: " + this._Lives);
                    }
                    Int_LoseLives_Boolean(_livesToLose) {
                        let _tempLives;
                        _tempLives = this._Lives - _livesToLose;
                        if(_tempLives < 0) {
                            return false;
                        }
                        this._Lives = _tempLives;
                        return true;
                    }
                }
                let _john;
                _john = new _Pet("john", 8);
                _john.Empty_Info_Void();
                let _sam;
                _sam = new _Cat("sam", 1, 9);
                _sam.Empty_Info_Void();
                _sam.Empty_Lives_Void();
                let _result;
                _result = _sam.Int_LoseLives_Boolean(1);
                console.log(_result);
                _sam.Empty_Lives_Void();
                _result = _sam.Int_LoseLives_Boolean(67);
                console.log(_result);
                _sam.Empty_Lives_Void();
                
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
                b = 4;

                while(true)
                {
                    println(b);

                    b = b - 1;

                    if(b < 0)
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
                let _a;
                _a = 0;
                while(_a < 5) {
                    console.log(_a);
                    _a = _a + 1;
                }
                console.log("Do while loop counter:");
                let _b;
                _b = 4;
                while(true) {
                    console.log(_b);
                    _b = _b - 1;
                    if(_b < 0) {
                        break;
                    }
                }
                console.log("String loop counter with a's:");
                let _text;
                _text = "";
                while(_text != "aaaa") {
                    if(_text == "aa") {
                        console.log("double aa, yaya");
                    }
                    console.log(_text);
                    _text = _text + "a";
                }
                
                """;

            AssertHelperExpectedResultCode(expected, code);
        }
    }
}
