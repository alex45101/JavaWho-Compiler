using JavaWhoCompiler;
using System.Linq.Expressions;

namespace CompilerTests
{
    public class ParserTests
    {
        [Fact]
        public void EmptyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");
            
            AST root = Parser.Parse(tokens);

            Assert.Null(root); //maybe should not return null but an empty
        }
    }
}
