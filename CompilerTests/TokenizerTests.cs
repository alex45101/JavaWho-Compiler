using System.Reflection.Metadata;
using JavaWhoCompiler;

namespace CompilerTests
{
    public class TokenizerTests
    {
        [Theory]
        [InlineData("_x")]
        [InlineData("x")]
        [InlineData("x_")]
        [InlineData("small_num")]
        [InlineData("while_text")]
        [InlineData("Cat")]
        [InlineData("cat09")]
        public void IdentifierTokensTest(string text)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(text);

            IToken token = tokens.First();

            Assert.Equal(text, token.Value);
            Assert.IsType<IdentifierToken>(token);
        }

        [Fact]
        public void UnknownTokenTest()
        {
            Assert.Throws<InvalidTokenException>(() => Tokenizer.Tokenize("$"));
        }

        [Fact]
        public void NoTokensTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");

            Assert.Empty(tokens);
        }

        [Fact]
        public void NoTokensWhitespaceTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("     ");

            Assert.Empty(tokens);
        }

        [Fact]
        public void StringTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("\"Hello World!\"");

            IToken token = tokens.First();

            Assert.Equal("\"Hello World!\"", token.Value);
            Assert.IsType<StringToken>(token);
        }

        [Fact]
        public void NumberTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("425");

            IToken token = tokens.First();

            Assert.Equal("425", token.Value);
            Assert.IsType<NumberToken>(token);

            Assert.Equal(425, (token as NumberToken).Number);
        }

        [Fact]
        public void ThisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this");

            IToken token = tokens.First();

            Assert.Equal("this", token.Value);
            Assert.IsType<ThisToken>(token);
        }

        [Fact]
        public void TrueTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("true");

            IToken token = tokens.First();

            Assert.Equal("true", token.Value);
            Assert.IsType<TrueToken>(token);
        }

        [Fact]
        public void FalseTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("false");

            IToken token = tokens.First();

            Assert.Equal("false", token.Value);
            Assert.IsType<FalseToken>(token);
        }

        [Fact]
        public void NewTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("new");

            IToken token = tokens.First();

            Assert.Equal("new", token.Value);
            Assert.IsType<NewToken>(token);
        }

        [Fact]
        public void InitTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("init");

            IToken token = tokens.First();

            Assert.Equal("init", token.Value);
            Assert.IsType<InitToken>(token);
        }

        [Fact]
        public void ClassTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("class");

            IToken token = tokens.First();

            Assert.Equal("class", token.Value);
            Assert.IsType<ClassToken>(token);
        }

        [Fact]
        public void SuperTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("super");

            IToken token = tokens.First();

            Assert.Equal("super", token.Value);
            Assert.IsType<SuperToken>(token);
        }

        [Fact]
        public void WhileTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("while");

            IToken token = tokens.First();

            Assert.Equal("while", token.Value);
            Assert.IsType<WhileToken>(token);
        }

        [Fact]
        public void BreakTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("break");

            IToken token = tokens.First();

            Assert.Equal("break", token.Value);
            Assert.IsType<BreakToken>(token);
        }

        [Fact]
        public void IfTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("if");

            IToken token = tokens.First();

            Assert.Equal("if", token.Value);
            Assert.IsType<IfToken>(token);
        }

        [Fact]
        public void ElseTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("else");

            IToken token = tokens.First();

            Assert.Equal("else", token.Value);
            Assert.IsType<ElseToken>(token);
        }

        [Fact]
        public void VoidTypeTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Void");

            IToken token = tokens.First();

            Assert.Equal("Void", token.Value);
            Assert.IsType<VoidTypeToken>(token);
        }

        [Fact]
        public void MethodTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("method");

            IToken token = tokens.First();

            Assert.Equal("method", token.Value);
            Assert.IsType<MethodToken>(token);
        }

        [Fact]
        public void ReturnTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return");

            IToken token = tokens.First();

            Assert.Equal("return", token.Value);
            Assert.IsType<ReturnToken>(token);
        }

        [Fact]
        public void ExtendsTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("extends");

            IToken token = tokens.First();

            Assert.Equal("extends", token.Value);
            Assert.IsType<ExtendsToken>(token);
        }

        [Fact]
        public void AddOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("+");

            IToken token = tokens.First();

            Assert.Equal("+", token.Value);
            Assert.IsType<AddOperatorToken>(token);
        }

        [Fact]
        public void SubtractOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("-");

            IToken token = tokens.First();

            Assert.Equal("-", token.Value);
            Assert.IsType<SubtractOperatorToken>(token);
        }

        [Fact]
        public void MultiplyOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("*");

            IToken token = tokens.First();

            Assert.Equal("*", token.Value);
            Assert.IsType<MultiplyOperatorToken>(token);
        }

        [Fact]
        public void DivideOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("/");

            IToken token = tokens.First();

            Assert.Equal("/", token.Value);
            Assert.IsType<DivideOperatorToken>(token);
        }

        [Fact]
        public void LessThanOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("<");

            IToken token = tokens.First();

            Assert.Equal("<", token.Value);
            Assert.IsType<LessThanOperatorToken>(token);
        }

        [Fact]
        public void EqualsOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("==");

            IToken token = tokens.First();

            Assert.Equal("==", token.Value);
            Assert.IsType<EqualsOperatorToken>(token);
        }

        [Fact]
        public void NotEqualsOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("!=");

            IToken token = tokens.First();

            Assert.Equal("!=", token.Value);
            Assert.IsType<NotEqualsOperatorToken>(token);
        }

        [Fact]
        public void AssignmentOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("=");

            IToken token = tokens.First();

            Assert.Equal("=", token.Value);
            Assert.IsType<AssignmentOperatorToken>(token);
        }

        [Fact]
        public void OpenParenthesisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(");

            IToken token = tokens.First();

            Assert.Equal("(", token.Value);
            Assert.IsType<OpenParenthesisToken>(token);
        }

        [Fact]
        public void CloseParenthesisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(")");

            IToken token = tokens.First();

            Assert.Equal(")", token.Value);
            Assert.IsType<CloseParenthesisToken>(token);
        }

        [Fact]
        public void SemiColonTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(";");

            IToken token = tokens.First();

            Assert.Equal(";", token.Value);
            Assert.IsType<SemiColonToken>(token);
        }

        [Fact]
        public void CommaTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(",");

            IToken token = tokens.First();

            Assert.Equal(",", token.Value);
            Assert.IsType<CommaToken>(token);
        }

        [Fact]
        public void DotTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(".");

            IToken token = tokens.First();

            Assert.Equal(".", token.Value);
            Assert.IsType<DotToken>(token);
        }


        private void AssertTokenEqualIgnoreLinePos(IToken expected, IToken actual) {
            Assert.Equal(expected.GetType(), actual.GetType());
            Assert.Equal(expected.Value, actual.Value);
        }

        private void AssertTokenListEqualIgnoreLinePos(IEnumerable<IToken> expected, IEnumerable<IToken> actual) {
            Assert.Equal(expected.Count(), actual.Count());

            foreach(var (i, expectedToken) in expected.Index()) {
                var actualToken = actual.ElementAt(i);
                AssertTokenEqualIgnoreLinePos(expectedToken, actualToken);
            }
        }

        private void AssertTokenListEqual(IEnumerable<IToken> expected, IEnumerable<IToken> actual) {
            Assert.Equal(expected.Count(), actual.Count());

            foreach(var (i, expectedToken) in expected.Index()) {
                var actualToken = actual.ElementAt(i);
                Assert.Equal(expectedToken, actualToken);
            }
        }

        [Fact]
        public void ClassDefExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal { init() {} method speak() Void { return println(0 == 5); }");
            List<IToken> expected = [
                new ClassToken("class", 0, 0),  new IdentifierToken("Bear", 0, 0), new ExtendsToken("extends", 0, 0),  new IdentifierToken("Animal", 0, 0),
                     new OpenCurlyBracketToken("{", 0, 0),

                new InitToken("init", 0, 0), new OpenParenthesisToken("(", 0, 0), new CloseParenthesisToken(")", 0, 0),
                 new OpenCurlyBracketToken("{", 0, 0), new CloseCurlyBracketToken("}", 0, 0),

                new MethodToken("method", 0, 0),  new IdentifierToken("speak", 0, 0), 
                    new OpenParenthesisToken("(", 0, 0), new CloseParenthesisToken(")", 0, 0),
                     new VoidTypeToken("Void", 0, 0),  new OpenCurlyBracketToken("{", 0, 0),

                new ReturnToken("return", 0, 0), 
                    new PrintLnToken("println", 0, 0), new OpenParenthesisToken("(", 0, 0), new NumberToken("0", 0, 0), 
                     new EqualsOperatorToken("==", 0, 0),  new NumberToken("5", 0, 0),
                    new CloseParenthesisToken(")", 0, 0),
                    new SemiColonToken(";", 0, 0),

                new CloseCurlyBracketToken("}", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }

        [Fact]
        public void ClassDefCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal{init(){}method speak()Void{return println(0==5);}");
            List<IToken> expected = [
                new ClassToken("class", 0, 0),  new IdentifierToken("Bear", 0, 0), 
                    new ExtendsToken("extends", 0, 0),  new IdentifierToken("Animal", 0, 0),
                    new OpenCurlyBracketToken("{", 0, 0),
                new InitToken("init", 0, 0), new OpenParenthesisToken("(", 0, 0), new CloseParenthesisToken(")", 0, 0),
                new OpenCurlyBracketToken("{", 0, 0), new CloseCurlyBracketToken("}", 0, 0),
                new MethodToken("method", 0, 0),  new IdentifierToken("speak", 0, 0), 
                    new OpenParenthesisToken("(", 0, 0), new CloseParenthesisToken(")", 0, 0),
                    new VoidTypeToken("Void", 0, 0), new OpenCurlyBracketToken("{", 0, 0),
                new ReturnToken("return", 0, 0), 
                    new PrintLnToken("println", 0, 0), new OpenParenthesisToken("(", 0, 0),
                    new NumberToken("0", 0, 0), new EqualsOperatorToken("==", 0, 0), new NumberToken("5", 0, 0),
                    new CloseParenthesisToken(")", 0, 0),
                    new SemiColonToken(";", 0, 0),
                new CloseCurlyBracketToken("}", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }


        [Fact]
        public void CallsAndMathOperatorsExampleTest() {
            var tokens = Tokenizer.Tokenize("Int x; x = 5; Int y; y = 3; Int z; z = math_util.square(x + 3); Int a; a = z * x / y - x;");
            List<IToken> expected = [
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("x", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("x", 0, 0),  new AssignmentOperatorToken("=", 0, 0),  new NumberToken("5", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("y", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("y", 0, 0),  new AssignmentOperatorToken("=", 0, 0),  new NumberToken("3", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("z", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("z", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("math_util", 0, 0), new DotToken(".", 0, 0), new IdentifierToken("square", 0, 0), new OpenParenthesisToken("(", 0, 0),
                        new IdentifierToken("x", 0, 0),  
                        new AddOperatorToken("+", 0, 0), 
                        new NumberToken("3", 0, 0),
                    new CloseParenthesisToken(")", 0, 0),
                new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("a", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("a", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("z", 0, 0),  new MultiplyOperatorToken("*", 0, 0),  new IdentifierToken("x", 0, 0),
                     new DivideOperatorToken("/", 0, 0),  new IdentifierToken("y", 0, 0), 
                    new SubtractOperatorToken("-", 0, 0),  new IdentifierToken("x", 0, 0), new SemiColonToken(";", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }

        [Fact]
        public void CallsAndMathOperatorsCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("Int x;x=5;Int y;y=3;Int z;z=math_util.square(x+3);Int a;a=z*x/y-x;");
            List<IToken> expected = [
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("x", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("x", 0, 0),  new AssignmentOperatorToken("=", 0, 0),  new NumberToken("5", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("y", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("y", 0, 0),  new AssignmentOperatorToken("=", 0, 0),  new NumberToken("3", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("z", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("z", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("math_util", 0, 0), new DotToken(".", 0, 0), new IdentifierToken("square", 0, 0), new OpenParenthesisToken("(", 0, 0),
                        new IdentifierToken("x", 0, 0),  
                        new AddOperatorToken("+", 0, 0), 
                        new NumberToken("3", 0, 0),
                    new CloseParenthesisToken(")", 0, 0),
                new SemiColonToken(";", 0, 0),
                new IdentifierToken("Int", 0, 0),  new IdentifierToken("a", 0, 0), new SemiColonToken(";", 0, 0),
                new IdentifierToken("a", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("z", 0, 0),  new MultiplyOperatorToken("*", 0, 0),  new IdentifierToken("x", 0, 0),
                     new DivideOperatorToken("/", 0, 0),  new IdentifierToken("y", 0, 0), 
                    new SubtractOperatorToken("-", 0, 0),  new IdentifierToken("x", 0, 0), new SemiColonToken(";", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }

        [Fact]
        public void IfElseExampleTest() {
            var tokens = Tokenizer.Tokenize("if(x < 7) { println(\"hello world\")} else { x = x + 1; }");
            List<IToken> expected = [
                new IfToken("if", 0, 0), new OpenParenthesisToken("(", 0, 0),
                new IdentifierToken("x", 0, 0),  new LessThanOperatorToken("<", 0, 0),
                     new NumberToken("7", 0, 0),
                new CloseParenthesisToken(")", 0, 0),
                new OpenCurlyBracketToken("{", 0, 0),
                new PrintLnToken("println", 0, 0), new OpenParenthesisToken("(", 0, 0),
                    new StringToken("\"hello world\"", 0, 0), new CloseParenthesisToken(")", 0, 0), new CloseCurlyBracketToken("}", 0, 0),
                new ElseToken("else", 0, 0),
                new OpenCurlyBracketToken("{", 0, 0),
                new IdentifierToken("x", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("x", 0, 0),  new AddOperatorToken("+", 0, 0),
                     new NumberToken("1", 0, 0), new SemiColonToken(";", 0, 0),
                     new CloseCurlyBracketToken("}", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }

        [Fact]
        public void IfElseCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("if(x<7){println(\"hello world\")}else{x=x+1;}");
            List<IToken> expected = [
                new IfToken("if", 0, 0), new OpenParenthesisToken("(", 0, 0),
                new IdentifierToken("x", 0, 0), new LessThanOperatorToken("<", 0, 0), new NumberToken("7", 0, 0),
                new CloseParenthesisToken(")", 0, 0),
                new OpenCurlyBracketToken("{", 0, 0),
                new PrintLnToken("println", 0, 0), new OpenParenthesisToken("(", 0, 0),
                    new StringToken("\"hello world\"", 0, 0), new CloseParenthesisToken(")", 0, 0), new CloseCurlyBracketToken("}", 0, 0),
                new ElseToken("else", 0, 0),
                new OpenCurlyBracketToken("{", 0, 0),
                new IdentifierToken("x", 0, 0),  new AssignmentOperatorToken("=", 0, 0), 
                    new IdentifierToken("x", 0, 0),  new AddOperatorToken("+", 0, 0),
                     new NumberToken("1", 0, 0), new SemiColonToken(";", 0, 0),
                     new CloseCurlyBracketToken("}", 0, 0),
            ];

            AssertTokenListEqualIgnoreLinePos(expected, tokens);
        }

        [Theory]
        [InlineData("$x = 5")] // $ not valid
        [InlineData("!(x == 7)")] // ! not valid
        [InlineData("y > 9")] // > not valid
        public void ErrorOnUnkownSymbolTest(string sequence) {
            Assert.Throws<InvalidTokenException>(() => Tokenizer.Tokenize(sequence));
        }

        [Theory]
        [InlineData("""
                x = 5;
                x + 2;

                println("Hello world");
                """)]
        public void LineNumPosTest(string code) {
            var tokens = Tokenizer.Tokenize(code);
            List<IToken> expected = [
                new IdentifierToken("x", 1, 1), new AssignmentOperatorToken("=", 1, 3), new NumberToken("5", 1, 5), new SemiColonToken(";", 1, 6),
                new IdentifierToken("x", 2, 1), new AddOperatorToken("+", 2, 3), new NumberToken("2", 2, 5), new SemiColonToken(";", 2, 6),

                new PrintLnToken("println", 4, 1), new OpenParenthesisToken("(", 4, 8), new StringToken("\"Hello world\"", 4, 9), new CloseParenthesisToken(")", 4, 22), 
                            new SemiColonToken(";", 4, 23),
            ];

            AssertTokenListEqual(expected, tokens);
        }
    }
}
