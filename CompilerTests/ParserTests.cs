using JavaWhoCompiler;

namespace CompilerTests
{
    public class ParserTests
    {
        public static IEnumerable<object[]> BinaryExpressionData()
        {
            yield return new object[] {
                "a < 5;",
                //new VariableExpression("a"),
                //OperatorType.LessThan,
                //new IntLiteralNode(5)
            };

            yield return new object[] {
                "a == 5;",
                //new VariableExpression("a"),
                //OperatorType.Equal,
                //new IntLiteralNode(5)
            };

            yield return new object[] {
                "a + 5;",
                //new VariableExpression("a"),
                //OperatorType.Add,
                //new IntLiteralNode(5)
            };

            yield return new object[] {
                "a - 5;",
                //new VariableExpression("a"),
                //OperatorType.Subtract,
                //new IntLiteralNode(5)
            };
        }

        [Fact]
        public void EmptyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");
            
            AST root = Parser.Parse(tokens);

            Assert.Null(root);
        }

        [Theory]
        [MemberData(nameof(BinaryExpressionData))]
        public void BinaryExpressionTests(string text)//, ParseNode expectedLeft, OperatorType expectedOperator, ParseNode expectedRight)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(text);

            AST root = Parser.Parse(tokens);

            BinaryParseNode binaryParseNode = Assert.IsType<BinaryParseNode>(root);
            //Assert.Equal(expectedLeft, binaryParseNode.Left);
            //Assert.Equal(expectedRight, binaryParseNode.Right);
            //Assert.Equal(expectedOperator, binaryParseNode.OperatorType);
        }
    }
}
