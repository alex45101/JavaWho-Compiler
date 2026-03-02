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

        [Fact]
        public void ClassDefExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal { init() {} method speak() Void { return println(0 == 5); }");
            Assert.Equal([
                new ClassToken("class"),  new IdentifierToken("Bear"), 
                    new ExtendsToken("extends"),  new IdentifierToken("Animal"),
                     new OpenCurlyBracketToken("{"),
                
                new InitToken("init"), new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                 new OpenCurlyBracketToken("{"), new CloseCurlyBracketToken("}"),
                
                new MethodToken("method"),  new IdentifierToken("speak"), 
                    new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                     new VoidTypeToken("Void"),  new OpenCurlyBracketToken("{"),
                
                new ReturnToken("return"), 
                    new PrintLnToken("println"), new OpenParenthesisToken("("), new NumberToken("0"), 
                     new EqualsOperatorToken("=="),  new NumberToken("5"),
                    new CloseParenthesisToken(")"),
                    new SemiColonToken(";"),
                
                new CloseCurlyBracketToken("}"),
            ], tokens);
        }

        [Fact]
        public void ClassDefCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal{init(){}method speak()Void{return println(0==5);}");
            Assert.Equal([
                new ClassToken("class"),  new IdentifierToken("Bear"), 
                    new ExtendsToken("extends"),  new IdentifierToken("Animal"),
                    new OpenCurlyBracketToken("{"),
                new InitToken("init"), new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                new OpenCurlyBracketToken("{"), new CloseCurlyBracketToken("}"),
                new MethodToken("method"),  new IdentifierToken("speak"), 
                    new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                    new VoidTypeToken("Void"), new OpenCurlyBracketToken("{"),
                new ReturnToken("return"), 
                    new PrintLnToken("println"), new OpenParenthesisToken("("),
                    new NumberToken("0"), new EqualsOperatorToken("=="), new NumberToken("5"),
                    new CloseParenthesisToken(")"),
                    new SemiColonToken(";"),
                new CloseCurlyBracketToken("}"),
            ], tokens);
        }


        [Fact]
        public void CallsAndMathOperatorsExampleTest() {
            var tokens = Tokenizer.Tokenize("Int x; x = 5; Int y; y = 3; Int z; z = math_util.square(x + 3); Int a; a = z * x / y - x;");
            Assert.Equal([
                new IdentifierToken("Int"),  new IdentifierToken("x"), new SemiColonToken(";"),
                
                new IdentifierToken("x"),  new AssignmentOperatorToken("="),  new NumberToken("5"), new SemiColonToken(";"),
                
                new IdentifierToken("Int"),  new IdentifierToken("y"), new SemiColonToken(";"),
                
                new IdentifierToken("y"),  new AssignmentOperatorToken("="),  new NumberToken("3"), new SemiColonToken(";"),
                
                new IdentifierToken("Int"),  new IdentifierToken("z"), new SemiColonToken(";"),
                
                new IdentifierToken("z"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("math_util"), new DotToken("."), new IdentifierToken("square"), new OpenParenthesisToken("("),
                        new IdentifierToken("x"),  
                        new AddOperatorToken("+"), 
                        new NumberToken("3"),
                    new CloseParenthesisToken(")"),
                new SemiColonToken(";"),
                
                new IdentifierToken("Int"),  new IdentifierToken("a"), new SemiColonToken(";"),
                
                new IdentifierToken("a"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("z"),  new MultiplyOperatorToken("*"),  new IdentifierToken("x"),
                     new DivideOperatorToken("/"),  new IdentifierToken("y"), 
                    new SubtractOperatorToken("-"),  new IdentifierToken("x"), new SemiColonToken(";"),
            ], tokens);
        }

        [Fact]
        public void CallsAndMathOperatorsCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("Int x;x=5;Int y;y=3;Int z;z=math_util.square(x+3);Int a;a=z*x/y-x;");
            Assert.Equal([
                new IdentifierToken("Int"),  new IdentifierToken("x"), new SemiColonToken(";"),
                new IdentifierToken("x"),  new AssignmentOperatorToken("="),  new NumberToken("5"), new SemiColonToken(";"),
                new IdentifierToken("Int"),  new IdentifierToken("y"), new SemiColonToken(";"),
                new IdentifierToken("y"),  new AssignmentOperatorToken("="),  new NumberToken("3"), new SemiColonToken(";"),
                new IdentifierToken("Int"),  new IdentifierToken("z"), new SemiColonToken(";"),
                new IdentifierToken("z"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("math_util"), new DotToken("."), new IdentifierToken("square"), new OpenParenthesisToken("("),
                        new IdentifierToken("x"),  
                        new AddOperatorToken("+"), 
                        new NumberToken("3"),
                    new CloseParenthesisToken(")"),
                new SemiColonToken(";"),
                new IdentifierToken("Int"),  new IdentifierToken("a"), new SemiColonToken(";"),
                new IdentifierToken("a"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("z"),  new MultiplyOperatorToken("*"),  new IdentifierToken("x"),
                     new DivideOperatorToken("/"),  new IdentifierToken("y"), 
                    new SubtractOperatorToken("-"),  new IdentifierToken("x"), new SemiColonToken(";"),
            ], tokens);
        }

        [Fact]
        public void IfElseExampleTest() {
            var tokens = Tokenizer.Tokenize("if(x < 7) { println(\"hello world\")} else { x = x + 1; }");
            Assert.Equal([
                new IfToken("if"), new OpenParenthesisToken("("),
                new IdentifierToken("x"),  new LessThanOperatorToken("<"),
                     new NumberToken("7"),
                new CloseParenthesisToken(")"),
                
                new OpenCurlyBracketToken("{"),
                
                new PrintLnToken("println"), new OpenParenthesisToken("("),
                    new StringToken("\"hello world\""), new CloseParenthesisToken(")"), new CloseCurlyBracketToken("}"),
                
                new ElseToken("else"),
                
                new OpenCurlyBracketToken("{"),
                
                new IdentifierToken("x"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("x"),  new AddOperatorToken("+"),
                     new NumberToken("1"), new SemiColonToken(";"),
                     new CloseCurlyBracketToken("}")
            ], tokens);
        }

        [Fact]
        public void IfElseCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("if(x<7){println(\"hello world\")}else{x=x+1;}");
            Assert.Equal([
                new IfToken("if"), new OpenParenthesisToken("("),
                new IdentifierToken("x"), new LessThanOperatorToken("<"), new NumberToken("7"),
                new CloseParenthesisToken(")"),
                new OpenCurlyBracketToken("{"),
                new PrintLnToken("println"), new OpenParenthesisToken("("),
                    new StringToken("\"hello world\""), new CloseParenthesisToken(")"), new CloseCurlyBracketToken("}"),
                new ElseToken("else"),
                new OpenCurlyBracketToken("{"),
                new IdentifierToken("x"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("x"),  new AddOperatorToken("+"),
                     new NumberToken("1"), new SemiColonToken(";"),
                     new CloseCurlyBracketToken("}")
            ], tokens);
        }

        [Theory]
        [InlineData("$x = 5")] // $ not valid
        [InlineData("!(x == 7)")] // ! not valid
        [InlineData("y > 9")] // > not valid
        public void ErrorOnUnkownSymbolTest(string sequence) {
            Assert.Throws<InvalidTokenException>(() => Tokenizer.Tokenize(sequence));
        }
    }
}
