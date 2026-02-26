using JavaWhoCompiler;

namespace CompilerTests
{
    public class ParserTests
    {
        public static IEnumerable<object[]> BinaryExpressionData()
        {
            yield return new object[] {
                "a < 5;",
                new IdentifiedNode("a"),
                OperatorType.LessThan,
                new IntLiteral(5)
            };

            yield return new object[] {
                "a == 5;",
                new IdentifiedNode("a"),
                OperatorType.Equal,
                new IntLiteral(5)
            };

            yield return new object[] {
                "a + 5;",
                new IdentifiedNode("a"),
                OperatorType.Add,
                new IntLiteral(5)
            };

            yield return new object[] {
                "a - 5;",
                new IdentifiedNode("a"),
                OperatorType.Subtract,
                new IntLiteral(5)
            };

            yield return new object[] {
                "5 == true;",
                new IntLiteral(5),
                OperatorType.Equal,
                new BooleanLiteral(true)
            };

            yield return new object[] {
                "5 != false;",
                new IntLiteral(5),
                OperatorType.NotEqual,
                new BooleanLiteral(false)
            };
        }

        [Fact]
        public void EmptyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");
            
            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);
            Assert.Empty(program.Classes);
            Assert.Empty(program.Statements);
        }

        [Theory]
        [MemberData(nameof(BinaryExpressionData))]
        public void BinaryExpressionTests(string text, AST expectedLeft, OperatorType expectedOperator, AST expectedRight)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(text);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);
            
            ExpStmt expStmt = Assert.IsType<ExpStmt>(program.Statements[0]);
            BinaryExpression binaryExpression = Assert.IsType<BinaryExpression>(expStmt.Exp);

            Assert.Equal(expectedLeft, binaryExpression.Left);
            Assert.Equal(expectedRight, binaryExpression.Right);
            Assert.Equal(expectedOperator, binaryExpression.OperatorType);
        }
    }
}
