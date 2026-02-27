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

        public static IEnumerable<object[]> NoSemicolonEndStmts()
        {
            yield return new object[] {"x + 2" };
            yield return new object[] {"Int x" };
            yield return new object[] { "x = 24" };
            yield return new object[] { "break" };
            yield return new object[] { "return" };
            yield return new object[] { "return 84" };
            yield return new object[] { "while(x == 1) x = x + 1" };
            yield return new object[] { "if(x == 1) x = x + 1" };
        }

        public static IEnumerable<object[]> NoSemicolonMidStmts()
        {
            yield return new object[] {"{ x + 2 }" };
            yield return new object[] {"{ Int x; x + 2 }" };
            yield return new object[] { "if(x != 2) { x = 21 }" };
            yield return new object[] { """
                                        while(x == 9) { 
                                            x = x + 1;
                                            if(x == 12) break
                                        }
                                        """ };
        }

        // private bool CollectionsEqual<T>(ICollection<T> col1, ICollection<T> col2) {
        //     if(col1.Count != col2.Count) return false;
        //
        //     foreach(var (i, item) in col1.Index()) {
        //         if(item.Equals(col2.ElementAt(i))) return false;
        //     }
        //
        //     return true;
        // }

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

        [Theory]
        [MemberData(nameof(NoSemicolonEndStmts))]
        public void ThrowOnNoSemicolonEndStmtTest(string code) {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<IndexOutOfRangeException>(() => Parser.Parse(tokens));
        }

        [Theory]
        [MemberData(nameof(NoSemicolonMidStmts))]
        public void ThrowOnNoSemicolonMidStmtTest(string code) {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        public void VardecStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Int x;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var vardecStatement = Assert.IsType<VardecStmt>(program.Statements[0]);

            var expected = new VardecStmt(
                    new IdentifiedNode("Int"),
                    new IdentifiedNode("x")
                    );

            Assert.Equal(expected, vardecStatement);
        }

        [Fact]
        public void AssignStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("x = 5;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var assignStatement = Assert.IsType<AssignStmt>(program.Statements[0]);

            var expected = new AssignStmt(
                    new IdentifiedNode("x"),
                    new IntLiteral(5)
                    );

            Assert.Equal(expected, assignStatement);
        }

        [Fact]
        public void WhileStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("while(x < 5) x = x + 1;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            WhileStmt whileStmt = Assert.IsType<WhileStmt>(program.Statements[0]);

            var expected = new WhileStmt(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                    ),
                new AssignStmt(
                    new IdentifiedNode("x"),
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.Add,
                        new IntLiteral(1)
                        )
                    )
            );

            Assert.Equal(expected, whileStmt);
        }

        [Fact]
        public void BreakStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("break;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            Assert.IsType<BreakStmt>(program.Statements[0]);

        }

        [Fact]
        public void ReturnVoidStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var returnStmt = Assert.IsType<ReturnStmt>(program.Statements[0]);
            Assert.Null(returnStmt.Val);
        }

        [Fact]
        public void ReturnStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return 5 + 8;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var returnStmt = Assert.IsType<ReturnStmt>(program.Statements[0]);

            var expectedVal = new BinaryExpression(
                    new IntLiteral(5),
                    OperatorType.Add,
                    new IntLiteral(8)
                    );

            Assert.Equal(expectedVal, returnStmt.Val);
        }

        [Fact]
        public void IfStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("if(x != 5) return v;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var ifStatement = Assert.IsType<IfStmt>(program.Statements[0]);

            var expected = 
                new IfStmt(
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.NotEqual,
                        new IntLiteral(5)
                        ),
                    new ReturnStmt(
                        new IdentifiedNode("v")
                    ),
                    null
                );

            Assert.Equal(expected, ifStatement);
        }

        [Fact]
        public void IfElseStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    if(x != 5)
                        return v;
                    else
                        return x;
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var ifStatement = Assert.IsType<IfStmt>(program.Statements[0]);

            var expected = 
                new IfStmt(
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.NotEqual,
                        new IntLiteral(5)
                        ),
                    new ReturnStmt(
                        new IdentifiedNode("v")
                    ),
                    new ReturnStmt(
                        new IdentifiedNode("x")
                        )
                );

            Assert.Equal(expected, ifStatement);
        }

        [Fact]
        public void BlockStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    {
                        Int x;
                        x = 5 + 2;
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var blockStatement = Assert.IsType<BlockStmt>(program.Statements[0]);

            List<AST> expectedStmts = [
                        new VardecStmt(
                                new IdentifiedNode("Int"),
                                new IdentifiedNode("x")
                                ),
                        new AssignStmt(
                            new IdentifiedNode("x"),
                            new BinaryExpression(
                                new IntLiteral(5),
                                OperatorType.Add,
                                new IntLiteral(2)
                            )
                        )
            ];

            Assert.Equal(expectedStmts.Count, blockStatement.Stmts.Count);

            foreach(var (i, stmt) in expectedStmts.Index()) {
                Assert.Equal(stmt, blockStatement.Stmts[i]);
            }
        }

        [Fact]
        public void MultipleStmtTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    Int x;
                    x = 5 + 2;
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);


            List<AST> expected = [
                        new VardecStmt(
                                new IdentifiedNode("Int"),
                                new IdentifiedNode("x")
                                ),
                        new AssignStmt(
                            new IdentifiedNode("x"),
                            new BinaryExpression(
                                new IntLiteral(5),
                                OperatorType.Add,
                                new IntLiteral(2)
                            )
                        ),
            ];

            Assert.Equal(expected.Count, program.Statements.Count);

            foreach(var (i, item) in expected.Index()) {
                Assert.Equal(item, program.Statements[i]);
            }
        }

    }
}
