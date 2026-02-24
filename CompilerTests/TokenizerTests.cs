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

            Assert.IsType<IdentifierToken>(tokens.First());
        }

        [Fact]
        public void NoTokensTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");

            Assert.Empty(tokens);
        }

        [Fact]
        public void WhiteSpaceTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("     ");

            Assert.IsType<WhiteSpaceToken>(tokens.First());
        }

        [Fact]
        public void StringTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("\"Hello World!\"");

            Assert.IsType<StringToken>(tokens.First());
        }

        [Fact]
        public void NumberTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("425");

            Assert.IsType<NumberToken>(tokens.First());
        }

        [Fact]
        public void ThisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this");

            Assert.IsType<ThisToken>(tokens.First());
        }

        [Fact]
        public void TrueTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("true");

            Assert.IsType<TrueToken>(tokens.First());
        }

        [Fact]
        public void FalseTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("false");

            Assert.IsType<FalseToken>(tokens.First());
        }

        [Fact]
        public void NewTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("new");

            Assert.IsType<NewToken>(tokens.First());
        }

        [Fact]
        public void InitTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("init");

            Assert.IsType<InitToken>(tokens.First());
        }

        [Fact]
        public void ClassTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("class");

            Assert.IsType<ClassToken>(tokens.First());
        }

        [Fact]
        public void SuperTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("super");

            Assert.IsType<SuperToken>(tokens.First());
        }

        [Fact]
        public void WhileTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("while");

            Assert.IsType<WhileToken>(tokens.First());
        }

        [Fact]
        public void BreakTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("break");

            Assert.IsType<BreakToken>(tokens.First());
        }

        [Fact]
        public void IfTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("if");

            Assert.IsType<IfToken>(tokens.First());
        }

        [Fact]
        public void ElseTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("else");

            Assert.IsType<ElseToken>(tokens.First());
        }

        [Fact]
        public void VoidTypeTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Void");

            Assert.IsType<VoidTypeToken>(tokens.First());
        }

        [Fact]
        public void MethodTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("method");

            Assert.IsType<MethodToken>(tokens.First());
        }

        [Fact]
        public void ReturnTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return");

            Assert.IsType<ReturnToken>(tokens.First());
        }

        [Fact]
        public void ExtendsTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("extends");

            Assert.IsType<ExtendsToken>(tokens.First());
        }

        [Fact]
        public void AddOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("+");

            Assert.IsType<AddOperatorToken>(tokens.First());
        }

        [Fact]
        public void SubtractOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("-");

            Assert.IsType<SubtractOperatorToken>(tokens.First());
        }

        [Fact]
        public void MultiplyOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("*");

            Assert.IsType<MultiplyOperatorToken>(tokens.First());
        }

        [Fact]
        public void DivideOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("/");

            Assert.IsType<DivideOperatorToken>(tokens.First());
        }

        [Fact]
        public void LessThanOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("<");

            Assert.IsType<LessThanOperatorToken>(tokens.First());
        }

        [Fact]
        public void EqualsOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("==");

            Assert.IsType<EqualsOperatorToken>(tokens.First());
        }

        [Fact]
        public void NotEqualsOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("!=");

            Assert.IsType<NotEqualsOperatorToken>(tokens.First());
        }

        [Fact]
        public void AssignmentOperatorTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("=");

            Assert.IsType<AssignmentOperatorToken>(tokens.First());
        }

        [Fact]
        public void OpenParenthesisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(");

            Assert.IsType<OpenParenthesisToken>(tokens.First());
        }

        [Fact]
        public void CloseParenthesisTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(")");

            Assert.IsType<CloseParenthesisToken>(tokens.First());
        }

        [Fact]
        public void SemiColonTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(";");

            Assert.IsType<SemiColonToken>(tokens.First());
        }

        [Fact]
        public void CommaTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(",");

            Assert.IsType<CommaToken>(tokens.First());
        }

        [Fact]
        public void DotTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(".");

            Assert.IsType<DotToken>(tokens.First());
        }

        [Fact]
        public void ClassDefExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal { init() {} method speak() Void { return println(0 == 5); }");
            Assert.Equal([
                new ClassToken("class"), new WhiteSpaceToken(" "), new IdentifierToken("Bear"), new WhiteSpaceToken(" "),
                    new ExtendsToken("extends"), new WhiteSpaceToken(" "), new IdentifierToken("Animal"),
                    new WhiteSpaceToken(" "), new OpenCurlyBracketToken("{"),
                new WhiteSpaceToken(" "),
                new InitToken("init"), new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                new WhiteSpaceToken(" "), new OpenCurlyBracketToken("{"), new CloseCurlyBracketToken("}"),
                new WhiteSpaceToken(" "),
                new MethodToken("method"), new WhiteSpaceToken(" "), new IdentifierToken("speak"), 
                    new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                    new WhiteSpaceToken(" "), new VoidTypeToken("Void"), new WhiteSpaceToken(" "), new OpenCurlyBracketToken("{"),
                new WhiteSpaceToken(" "),
                new ReturnToken("return"), new WhiteSpaceToken(" "),
                    new IdentifierToken("println"), new OpenParenthesisToken("("), new NumberToken("0"), 
                    new WhiteSpaceToken(" "), new EqualsOperatorToken("=="), new WhiteSpaceToken(" "), new NumberToken("5"),
                    new CloseParenthesisToken(")"),
                    new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new CloseCurlyBracketToken("}"),
            ], tokens);
        }

        [Fact]
        public void ClassDefCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("class Bear extends Animal{init(){}method speak()Void{return println(0==5);}");
            Assert.Equal([
                new ClassToken("class"), new WhiteSpaceToken(" "), new IdentifierToken("Bear"), new WhiteSpaceToken(" "),
                    new ExtendsToken("extends"), new WhiteSpaceToken(" "), new IdentifierToken("Animal"),
                    new OpenCurlyBracketToken("{"),
                new InitToken("init"), new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                new OpenCurlyBracketToken("{"), new CloseCurlyBracketToken("}"),
                new MethodToken("method"), new WhiteSpaceToken(" "), new IdentifierToken("speak"), 
                    new OpenParenthesisToken("("), new CloseParenthesisToken(")"),
                    new VoidTypeToken("Void"), new OpenCurlyBracketToken("{"),
                new ReturnToken("return"), new WhiteSpaceToken(" "),
                    new IdentifierToken("println"), new OpenParenthesisToken("("),
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
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("x"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("x"), new WhiteSpaceToken(" "), new AssignmentOperatorToken("="), new WhiteSpaceToken(" "), new NumberToken("5"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("y"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("y"), new WhiteSpaceToken(" "), new AssignmentOperatorToken("="), new WhiteSpaceToken(" "), new NumberToken("3"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("z"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("z"), new WhiteSpaceToken(" "), new AssignmentOperatorToken("="), new WhiteSpaceToken(" "),
                    new IdentifierToken("math_util"), new DotToken("."), new IdentifierToken("square"), new OpenParenthesisToken("("),
                        new IdentifierToken("x"), new WhiteSpaceToken(" "), 
                        new AddOperatorToken("+"), new WhiteSpaceToken(" "),
                        new NumberToken("3"),
                    new CloseParenthesisToken(")"),
                new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("a"), new SemiColonToken(";"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("a"), new WhiteSpaceToken(" "), new AssignmentOperatorToken("="), new WhiteSpaceToken(" "),
                    new IdentifierToken("z"), new WhiteSpaceToken(" "), new MultiplyOperatorToken("*"), new WhiteSpaceToken(" "), new IdentifierToken("x"),
                    new WhiteSpaceToken(" "), new DivideOperatorToken("/"), new WhiteSpaceToken(" "), new IdentifierToken("y"), new WhiteSpaceToken(" "),
                    new SubtractOperatorToken("-"), new WhiteSpaceToken(" "), new IdentifierToken("x"), new SemiColonToken(";"),
            ], tokens);
        }

        [Fact]
        public void CallsAndMathOperatorsCompactExampleTest() {
            var tokens = Tokenizer.Tokenize("Int x;x=5;Int y;y=3;Int z;z=math_util.square(x+3);Int a;a=z*x/y-x;");
            Assert.Equal([
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("x"), new SemiColonToken(";"),
                new IdentifierToken("x"),  new AssignmentOperatorToken("="),  new NumberToken("5"), new SemiColonToken(";"),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("y"), new SemiColonToken(";"),
                new IdentifierToken("y"),  new AssignmentOperatorToken("="),  new NumberToken("3"), new SemiColonToken(";"),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("z"), new SemiColonToken(";"),
                new IdentifierToken("z"),  new AssignmentOperatorToken("="), 
                    new IdentifierToken("math_util"), new DotToken("."), new IdentifierToken("square"), new OpenParenthesisToken("("),
                        new IdentifierToken("x"),  
                        new AddOperatorToken("+"), 
                        new NumberToken("3"),
                    new CloseParenthesisToken(")"),
                new SemiColonToken(";"),
                new IdentifierToken("Int"), new WhiteSpaceToken(" "), new IdentifierToken("a"), new SemiColonToken(";"),
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
                new IdentifierToken("x"), new WhiteSpaceToken(" "), new LessThanOperatorToken("<"),
                    new WhiteSpaceToken(" "), new NumberToken("7"),
                new CloseParenthesisToken(")"),
                new WhiteSpaceToken(" "),
                new OpenCurlyBracketToken("{"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("println"), new OpenParenthesisToken("("),
                    new StringToken("\"hello world\""), new CloseParenthesisToken(")"), new CloseCurlyBracketToken("}"),
                new WhiteSpaceToken(" "),
                new ElseToken("else"),
                new WhiteSpaceToken(" "),
                new OpenCurlyBracketToken("{"),
                new WhiteSpaceToken(" "),
                new IdentifierToken("x"), new WhiteSpaceToken(" "), new AssignmentOperatorToken("="), new WhiteSpaceToken(" "),
                    new IdentifierToken("x"), new WhiteSpaceToken(" "), new AddOperatorToken("+"),
                    new WhiteSpaceToken(" "), new NumberToken("1"), new SemiColonToken(";"),
                    new WhiteSpaceToken(" "), new CloseCurlyBracketToken("}")
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
                new IdentifierToken("println"), new OpenParenthesisToken("("),
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
