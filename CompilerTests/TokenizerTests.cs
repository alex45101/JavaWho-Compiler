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
        [InlineData("Int")]
        [InlineData("Boolean")]
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
        public void VoidTokenTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("void");

            Assert.IsType<VoidToken>(tokens.First());
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
    }
}
