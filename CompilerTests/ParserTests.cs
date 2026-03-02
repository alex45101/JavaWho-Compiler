using JavaWhoCompiler;
using System.Runtime.ExceptionServices;

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

            yield return new object[] {
                "a * 5;",
                new IdentifiedNode("a"),
                OperatorType.Multiply,
                new IntLiteral(5)
            };

            yield return new object[] {
                "10 / b;",
                new IntLiteral(10),
                OperatorType.Divide,
                new IdentifiedNode("b")
            };
        }

        public static IEnumerable<object[]> NoSemicolonEndStmts()
        {
            yield return new object[] { "x + 2" };
            yield return new object[] { "Int x" };
            yield return new object[] { "x = 24" };
            yield return new object[] { "break" };
            yield return new object[] { "return" };
            yield return new object[] { "return 84" };
            yield return new object[] { "while(x == 1) x = x + 1" };
            yield return new object[] { "if(x == 1) x = x + 1" };
        }

        public static IEnumerable<object[]> NoSemicolonMidStmts()
        {
            yield return new object[] { "{ x + 2 }" };
            yield return new object[] { "{ Int x; x + 2 }" };
            yield return new object[] { "if(x != 2) { x = 21 }" };
            yield return new object[] { """
                                        while(x == 9) { 
                                            x = x + 1;
                                            if(x == 12) break
                                        }
                                        """ };
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

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            BinaryExpression binaryExpression = Assert.IsType<BinaryExpression>(expStmt.Expression);

            Assert.Equal(expectedLeft, binaryExpression.Left);
            Assert.Equal(expectedRight, binaryExpression.Right);
            Assert.Equal(expectedOperator, binaryExpression.OperatorType);
        }

        [Theory]
        [MemberData(nameof(NoSemicolonEndStmts))]
        public void ThrowOnNoSemicolonEndStmtTest(string code)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<IndexOutOfRangeException>(() => Parser.Parse(tokens));
        }

        [Theory]
        [MemberData(nameof(NoSemicolonMidStmts))]
        public void ThrowOnNoSemicolonMidStmtTest(string code)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        public void VardecStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Int x;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var vardecStatement = Assert.IsType<VariableDeclarationStatement>(program.Statements[0]);

            var expected = new VariableDeclarationStatement(
                    new IdentifiedNode("Int"),
                    new IdentifiedNode("x")
                    );

            Assert.Equal(expected, vardecStatement);
        }

        [Fact]
        public void AssignStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("x = 5;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var assignStatement = Assert.IsType<AssignmentStatement>(program.Statements[0]);

            var expected = new AssignmentStatement(
                    new IdentifiedNode("x"),
                    new IntLiteral(5)
                    );

            Assert.Equal(expected, assignStatement);
        }

        [Fact]
        public void WhileStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("while(x < 5) x = x + 1;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            WhileStatement whileStmt = Assert.IsType<WhileStatement>(program.Statements[0]);

            var expected = new WhileStatement(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                    ),
                new AssignmentStatement(
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
        public void BreakStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("break;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            Assert.IsType<BreakStatement>(program.Statements[0]);

        }

        [Fact]
        public void ReturnVoidStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var returnStmt = Assert.IsType<ReturnStatement>(program.Statements[0]);
            Assert.Null(returnStmt.Val);
        }

        [Fact]
        public void ReturnStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return 5 + 8;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var returnStmt = Assert.IsType<ReturnStatement>(program.Statements[0]);

            var expectedVal = new BinaryExpression(
                    new IntLiteral(5),
                    OperatorType.Add,
                    new IntLiteral(8)
                    );

            Assert.Equal(expectedVal, returnStmt.Val);
        }

        [Fact]
        public void IfStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("if(x != 5) return v;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var ifStatement = Assert.IsType<IfStatement>(program.Statements[0]);

            var expected =
                new IfStatement(
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.NotEqual,
                        new IntLiteral(5)
                        ),
                    new ReturnStatement(
                        new IdentifiedNode("v")
                    ),
                    null
                );

            Assert.Equal(expected, ifStatement);
        }

        [Fact]
        public void IfElseStmtTest()
        {
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

            var ifStatement = Assert.IsType<IfStatement>(program.Statements[0]);

            var expected =
                new IfStatement(
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.NotEqual,
                        new IntLiteral(5)
                        ),
                    new ReturnStatement(
                        new IdentifiedNode("v")
                    ),
                    new ReturnStatement(
                        new IdentifiedNode("x")
                        )
                );

            Assert.Equal(expected, ifStatement);
        }

        [Fact]
        public void EmptyBlockStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("{}");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var blockStatement = Assert.IsType<BlockStatement>(program.Statements[0]);

            Assert.Empty(blockStatement.Statements);
        }

        [Fact]
        public void BlockStmtTest()
        {
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

            var blockStatement = Assert.IsType<BlockStatement>(program.Statements[0]);

            List<AST> expectedStmts = [
                        new VariableDeclarationStatement(
                                new IdentifiedNode("Int"),
                                new IdentifiedNode("x")
                                ),
                        new AssignmentStatement(
                            new IdentifiedNode("x"),
                            new BinaryExpression(
                                new IntLiteral(5),
                                OperatorType.Add,
                                new IntLiteral(2)
                            )
                        )
            ];

            Assert.Equal(expectedStmts.Count, blockStatement.Statements.Count);

            foreach (var (i, stmt) in expectedStmts.Index())
            {
                Assert.Equal(stmt, blockStatement.Statements[i]);
            }
        }

        [Fact]
        public void MultipleStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    Int x;
                    x = 5 + 2;
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);


            List<AST> expected = [
                        new VariableDeclarationStatement(
                                new IdentifiedNode("Int"),
                                new IdentifiedNode("x")
                                ),
                        new AssignmentStatement(
                            new IdentifiedNode("x"),
                            new BinaryExpression(
                                new IntLiteral(5),
                                OperatorType.Add,
                                new IntLiteral(2)
                            )
                        ),
            ];

            Assert.Equal(expected.Count, program.Statements.Count);

            foreach (var (i, item) in expected.Index())
            {
                Assert.Equal(item, program.Statements[i]);
            }
        }

        [Fact]
        public void OperatorPrecedenceMultiplyBeforeAddTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("2 + 3 * 4;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new IntLiteral(2),
                OperatorType.Add,
                new BinaryExpression(
                    new IntLiteral(3),
                    OperatorType.Multiply,
                    new IntLiteral(4)
                )
            );

            Assert.Equal(expected, expStmt.Expression);
        }

        [Fact]
        public void OperatorPrecedenceAddBeforeCompareTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a + 2 < b + 3;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a"),
                    OperatorType.Add,
                    new IntLiteral(2)
                ),
                OperatorType.LessThan,
                new BinaryExpression(
                    new IdentifiedNode("b"),
                    OperatorType.Add,
                    new IntLiteral(3)
                )
            );

            Assert.Equal(expected, expStmt.Expression);
        }

        [Fact]
        public void OperatorPrecedenceCompareBeforeEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a < 5 == b < 10;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                OperatorType.Equal,
                new BinaryExpression(
                    new IdentifiedNode("b"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                )
            );

            Assert.Equal(expected, expStmt.Expression);
        }

        [Fact]
        public void ComplexOperatorPrecedenceTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a * 2 + b / 3 - 4;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifiedNode("a"),
                        OperatorType.Multiply,
                        new IntLiteral(2)
                    ),
                    OperatorType.Add,
                    new BinaryExpression(
                        new IdentifiedNode("b"),
                        OperatorType.Divide,
                        new IntLiteral(3)
                    )
                ),
                OperatorType.Subtract,
                new IntLiteral(4)
            );

            Assert.Equal(expected, expStmt.Expression);
        }

        [Fact]
        public void SimpleIdentifierExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("x;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            IdentifiedNode identifier = Assert.IsType<IdentifiedNode>(expStmt.Expression);

            Assert.Equal("x", identifier.Value);
        }

        [Fact]
        public void SimpleIntLiteralExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("42;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            IntLiteral literal = Assert.IsType<IntLiteral>(expStmt.Expression);

            Assert.Equal(42, literal.Value);
        }

        [Fact]
        public void SimpleBooleanLiteralExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("true;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            BooleanLiteral literal = Assert.IsType<BooleanLiteral>(expStmt.Expression);

            Assert.True(literal.Value);
        }

        [Fact]
        public void NestedIfStatementsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x < 5)
                    if(y < 10)
                        x = 1;
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerIf = Assert.IsType<IfStatement>(program.Statements[0]);
            var innerIf = Assert.IsType<IfStatement>(outerIf.IfBody);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                outerIf.Guard
            );

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("y"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                ),
                innerIf.Guard
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("x"),
                    new IntLiteral(1)
                ),
                innerIf.IfBody
            );
        }

        [Fact]
        public void NestedWhileStatementsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                while(x < 5)
                    while(y < 10)
                        y = y + 1;
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerWhile = Assert.IsType<WhileStatement>(program.Statements[0]);
            var innerWhile = Assert.IsType<WhileStatement>(outerWhile.Statement);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                outerWhile.Guard
            );

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("y"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                ),
                innerWhile.Guard
            );

            var innerWhileBody = Assert.IsType<AssignmentStatement>(innerWhile.Statement);

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new BinaryExpression(
                        new IdentifiedNode("y"),
                        OperatorType.Add,
                        new IntLiteral(1)
                    )
                ),
                innerWhileBody
            );
        }

        [Fact]
        public void WhileWithBlockBodyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                while(x < 5) {
                    x = x + 1;
                    y = y + 2;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var whileStmt = Assert.IsType<WhileStatement>(program.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                whileStmt.Guard
            );

            var block = Assert.IsType<BlockStatement>(whileStmt.Statement);

            Assert.Equal(2, block.Statements.Count);

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("x"),
                    new BinaryExpression(
                        new IdentifiedNode("x"),
                        OperatorType.Add,
                        new IntLiteral(1)
                    )
                ),
                block.Statements[0]
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new BinaryExpression(
                        new IdentifiedNode("y"),
                        OperatorType.Add,
                        new IntLiteral(2)
                    )
                ),
                block.Statements[1]
            );
        }

        [Fact]
        public void IfWithBlockBodyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x == 5) {
                    x = 0;
                    y = 0;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var ifStmt = Assert.IsType<IfStatement>(program.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.Equal,
                    new IntLiteral(5)
                ),
                ifStmt.Guard
            );

            var block = Assert.IsType<BlockStatement>(ifStmt.IfBody);

            Assert.Equal(2, block.Statements.Count);

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("x"),
                    new IntLiteral(0)
                ),
                block.Statements[0]
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new IntLiteral(0)
                ),
                block.Statements[1]
            );

            Assert.Null(ifStmt.ElseBody);
        }

        [Fact]
        public void IfElseWithBlockBodiesTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x == 5) {
                    x = 0;
                } else {
                    x = 1;
                    y = 2;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var ifStmt = Assert.IsType<IfStatement>(program.Statements[0]);
            var ifBlock = Assert.IsType<BlockStatement>(ifStmt.IfBody);
            var elseBlock = Assert.IsType<BlockStatement>(ifStmt.ElseBody);

            Assert.Single(ifBlock.Statements);
            Assert.Equal(2, elseBlock.Statements.Count);
        }

        [Fact]
        public void ElseIfStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x < 5)
                    y = 1;
                else if(x < 10)
                    y = 2;
                else
                    y = 3;
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerIf = Assert.IsType<IfStatement>(program.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                outerIf.Guard
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new IntLiteral(1)
                ),
                outerIf.IfBody
            );

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                ),
                elseIfStmt.Guard
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new IntLiteral(2)
                ),
                elseIfStmt.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("y"),
                    new IntLiteral(3)
                ),
                elseIfStmt.ElseBody
            );
        }

        [Fact]
        public void MultipleElseIfStatementsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x == 1)
                    y = 10;
                else if(x == 2)
                    y = 20;
                else if(x == 3)
                    y = 30;
                else if(x == 4)
                    y = 40;
                else
                    y = 50;
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var firstIf = Assert.IsType<IfStatement>(program.Statements[0]);
            var secondIf = Assert.IsType<IfStatement>(firstIf.ElseBody);
            var thirdIf = Assert.IsType<IfStatement>(secondIf.ElseBody);
            var fourthIf = Assert.IsType<IfStatement>(thirdIf.ElseBody);

            Assert.Equal(
                new BinaryExpression(new IdentifiedNode("x"), OperatorType.Equal, new IntLiteral(1)),
                firstIf.Guard
            );
            Assert.Equal(
                new BinaryExpression(new IdentifiedNode("x"), OperatorType.Equal, new IntLiteral(2)),
                secondIf.Guard
            );
            Assert.Equal(
                new BinaryExpression(new IdentifiedNode("x"), OperatorType.Equal, new IntLiteral(3)),
                thirdIf.Guard
            );
            Assert.Equal(
                new BinaryExpression(new IdentifiedNode("x"), OperatorType.Equal, new IntLiteral(4)),
                fourthIf.Guard
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(10)),
                firstIf.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(20)),
                secondIf.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(30)),
                thirdIf.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(40)),
                fourthIf.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(50)),
                fourthIf.ElseBody
            );
        }

        [Fact]
        public void ElseIfWithBlocksTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x < 5) {
                    y = 1;
                    z = 1;
                } else if(x < 10) {
                    y = 2;
                    z = 2;
                } else {
                    y = 3;
                    z = 3;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerIf = Assert.IsType<IfStatement>(program.Statements[0]);
            var ifBlock = Assert.IsType<BlockStatement>(outerIf.IfBody);

            Assert.Equal(2, ifBlock.Statements.Count);

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(1)),
                ifBlock.Statements[0]
            );
            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("z"), new IntLiteral(1)),
                ifBlock.Statements[1]
            );

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);
            var elseIfBlock = Assert.IsType<BlockStatement>(elseIfStmt.IfBody);

            Assert.Equal(2, elseIfBlock.Statements.Count);

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(2)),
                elseIfBlock.Statements[0]
            );
            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("z"), new IntLiteral(2)),
                elseIfBlock.Statements[1]
            );

            var finalElseBlock = Assert.IsType<BlockStatement>(elseIfStmt.ElseBody);

            Assert.Equal(2, finalElseBlock.Statements.Count);

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(3)),
                finalElseBlock.Statements[0]
            );
            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("z"), new IntLiteral(3)),
                finalElseBlock.Statements[1]
            );
        }

        [Fact]
        public void NestedIfInsideElseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x < 5)
                    y = 1;
                else {
                    if(z < 10)
                        y = 2;
                    else
                        y = 3;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerIf = Assert.IsType<IfStatement>(program.Statements[0]);

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(1)),
                outerIf.IfBody
            );

            var elseBlock = Assert.IsType<BlockStatement>(outerIf.ElseBody);

            Assert.Single(elseBlock.Statements);

            var nestedIf = Assert.IsType<IfStatement>(elseBlock.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("z"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                ),
                nestedIf.Guard
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(2)),
                nestedIf.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(new IdentifiedNode("y"), new IntLiteral(3)),
                nestedIf.ElseBody
            );
        }

        [Fact]
        public void ComplexNestedIfElseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(x < 5) {
                    if(y < 3)
                        z = 1;
                    else
                        z = 2;
                } else if(x < 10) {
                    if(y < 3)
                        z = 3;
                } else {
                    z = 4;
                }
                """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var outerIf = Assert.IsType<IfStatement>(program.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(5)
                ),
                outerIf.Guard
            );

            var ifBlock = Assert.IsType<BlockStatement>(outerIf.IfBody);

            Assert.Single(ifBlock.Statements);

            var nestedIfInIfBlock = Assert.IsType<IfStatement>(ifBlock.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("y"),
                    OperatorType.LessThan,
                    new IntLiteral(3)
                ),
                nestedIfInIfBlock.Guard
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("z"),
                    new IntLiteral(1)
                ),
                nestedIfInIfBlock.IfBody
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("z"),
                    new IntLiteral(2)
                ),
                nestedIfInIfBlock.ElseBody
            );

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("x"),
                    OperatorType.LessThan,
                    new IntLiteral(10)
                ),
                elseIfStmt.Guard
            );

            var elseIfBlock = Assert.IsType<BlockStatement>(elseIfStmt.IfBody);

            Assert.Single(elseIfBlock.Statements);

            var nestedIfInElseIfBlock = Assert.IsType<IfStatement>(elseIfBlock.Statements[0]);

            Assert.Equal(
                new BinaryExpression(
                    new IdentifiedNode("y"),
                    OperatorType.LessThan,
                    new IntLiteral(3)
                ),
                nestedIfInElseIfBlock.Guard
            );

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("z"),
                    new IntLiteral(3)
                ),
                nestedIfInElseIfBlock.IfBody
            );

            Assert.Null(nestedIfInElseIfBlock.ElseBody);

            var finalElseBlock = Assert.IsType<BlockStatement>(elseIfStmt.ElseBody);

            Assert.Single(finalElseBlock.Statements);

            Assert.Equal(
                new AssignmentStatement(
                    new IdentifiedNode("z"),
                    new IntLiteral(4)
                ),
                finalElseBlock.Statements[0]
            );
        }

        [Fact]
        public void SimpleMethodCallExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.run();");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("run", methodCall.Name);
            Assert.Empty(methodCall.Arguments);
        }

        [Fact]
        public void SimpleChainedMethodCallExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a.run().jump().walk();");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var finalMethodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);
            var innerMethodCall = Assert.IsType<MethodCallStatement>(finalMethodCall.Target);
            var outterMethodCall = Assert.IsType<MethodCallStatement>(innerMethodCall.Target);

            var identifier = Assert.IsType<IdentifiedNode>(outterMethodCall.Target);

            Assert.Equal("a", identifier.Value);

            Assert.Equal("run", outterMethodCall.Name);
            Assert.Empty(outterMethodCall.Arguments);

            Assert.Equal("jump", innerMethodCall.Name);
            Assert.Empty(innerMethodCall.Arguments);

            Assert.Equal("walk", finalMethodCall.Name);
            Assert.Empty(finalMethodCall.Arguments);
        }

        [Fact]
        public void MethodCallWithSingleIntArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(42);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            var target = Assert.IsType<IdentifiedNode>(methodCall.Target);
            Assert.Equal("obj", target.Value);
            Assert.Equal("calculate", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<IntLiteral>(methodCall.Arguments[0]);
            Assert.Equal(42, arg.Value);
        }

        [Fact]
        public void MethodCallWithSingleBooleanArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.setFlag(true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("setFlag", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[0]);
            Assert.True(arg.Value);
        }

        [Fact]
        public void MethodCallWithSingleStringArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.print(\"hello\");");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("print", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<StringLiteral>(methodCall.Arguments[0]);
            Assert.Equal("\"hello\"", arg.Value);
        }

        [Fact]
        public void MethodCallWithSingleIdentifierArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.process(x);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<IdentifiedNode>(methodCall.Arguments[0]);
            Assert.Equal("x", arg.Value);
        }

        [Fact]
        public void MethodCallWithMultipleArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(5, x, true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("calculate", methodCall.Name);
            Assert.Equal(3, methodCall.Arguments.Count);

            var firstArg = Assert.IsType<IntLiteral>(methodCall.Arguments[0]);
            Assert.Equal(5, firstArg.Value);

            var secondArg = Assert.IsType<IdentifiedNode>(methodCall.Arguments[1]);
            Assert.Equal("x", secondArg.Value);

            var thirdArg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[2]);
            Assert.True(thirdArg.Value);
        }

        [Fact]
        public void MethodCallWithBinaryExpressionArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(x + 5);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("calculate", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<BinaryExpression>(methodCall.Arguments[0]);
            Assert.Equal(OperatorType.Add, arg.OperatorType);

            var left = Assert.IsType<IdentifiedNode>(arg.Left);
            Assert.Equal("x", left.Value);

            var right = Assert.IsType<IntLiteral>(arg.Right);
            Assert.Equal(5, right.Value);
        }

        [Fact]
        public void MethodCallWithComplexBinaryExpressionArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.evaluate(a * 2 + b);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("evaluate", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<BinaryExpression>(methodCall.Arguments[0]);
            Assert.Equal(OperatorType.Add, arg.OperatorType);

            var leftSide = Assert.IsType<BinaryExpression>(arg.Left);
            Assert.Equal(OperatorType.Multiply, leftSide.OperatorType);
        }

        [Fact]
        public void MethodCallWithNestedMethodCallArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.process(helper.getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var nestedCall = Assert.IsType<MethodCallStatement>(methodCall.Arguments[0]);
            Assert.Equal("getValue", nestedCall.Name);
            Assert.Empty(nestedCall.Arguments);

            var nestedTarget = Assert.IsType<IdentifiedNode>(nestedCall.Target);
            Assert.Equal("helper", nestedTarget.Value);
        }

        [Fact]
        public void MethodCallWithMultipleNestedMethodCallArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.compare(a.getValue(), b.getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("compare", methodCall.Name);
            Assert.Equal(2, methodCall.Arguments.Count);

            var firstNestedCall = Assert.IsType<MethodCallStatement>(methodCall.Arguments[0]);
            Assert.Equal("getValue", firstNestedCall.Name);
            var firstTarget = Assert.IsType<IdentifiedNode>(firstNestedCall.Target);
            Assert.Equal("a", firstTarget.Value);

            var secondNestedCall = Assert.IsType<MethodCallStatement>(methodCall.Arguments[1]);
            Assert.Equal("getValue", secondNestedCall.Name);
            var secondTarget = Assert.IsType<IdentifiedNode>(secondNestedCall.Target);
            Assert.Equal("b", secondTarget.Value);
        }

        [Fact]
        public void MethodCallWithChainedMethodCallArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.process(helper.getManager().getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var nestedCall = Assert.IsType<MethodCallStatement>(methodCall.Arguments[0]);
            Assert.Equal("getValue", nestedCall.Name);

            var innerCall = Assert.IsType<MethodCallStatement>(nestedCall.Target);
            Assert.Equal("getManager", innerCall.Name);

            var innerTarget = Assert.IsType<IdentifiedNode>(innerCall.Target);
            Assert.Equal("helper", innerTarget.Value);
        }

        [Fact]
        public void MethodCallWithMixedComplexArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(x + 5, helper.getValue(), true, 42);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("calculate", methodCall.Name);
            Assert.Equal(4, methodCall.Arguments.Count);

            var firstArg = Assert.IsType<BinaryExpression>(methodCall.Arguments[0]);
            Assert.Equal(OperatorType.Add, firstArg.OperatorType);

            var secondArg = Assert.IsType<MethodCallStatement>(methodCall.Arguments[1]);
            Assert.Equal("getValue", secondArg.Name);

            var thirdArg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[2]);
            Assert.True(thirdArg.Value);

            var fourthArg = Assert.IsType<IntLiteral>(methodCall.Arguments[3]);
            Assert.Equal(42, fourthArg.Value);
        }

        [Fact]
        public void ChainedMethodCallWithArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.first(5).second(x).third(true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var finalCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("third", finalCall.Name);
            Assert.Single(finalCall.Arguments);
            var finalArg = Assert.IsType<BooleanLiteral>(finalCall.Arguments[0]);
            Assert.True(finalArg.Value);

            var middleCall = Assert.IsType<MethodCallStatement>(finalCall.Target);
            Assert.Equal("second", middleCall.Name);
            Assert.Single(middleCall.Arguments);
            var middleArg = Assert.IsType<IdentifiedNode>(middleCall.Arguments[0]);
            Assert.Equal("x", middleArg.Value);

            var firstCall = Assert.IsType<MethodCallStatement>(middleCall.Target);
            Assert.Equal("first", firstCall.Name);
            Assert.Single(firstCall.Arguments);
            var firstArg = Assert.IsType<IntLiteral>(firstCall.Arguments[0]);
            Assert.Equal(5, firstArg.Value);

            var target = Assert.IsType<IdentifiedNode>(firstCall.Target);
            Assert.Equal("obj", target.Value);
        }

        [Fact]
        public void MethodCallWithMethodCallWithArgumentsAsArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.outer(inner.compute(5, x));");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var outerCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.Equal("outer", outerCall.Name);
            Assert.Single(outerCall.Arguments);

            var innerCall = Assert.IsType<MethodCallStatement>(outerCall.Arguments[0]);
            Assert.Equal("compute", innerCall.Name);
            Assert.Equal(2, innerCall.Arguments.Count);

            var firstInnerArg = Assert.IsType<IntLiteral>(innerCall.Arguments[0]);
            Assert.Equal(5, firstInnerArg.Value);

            var secondInnerArg = Assert.IsType<IdentifiedNode>(innerCall.Arguments[1]);
            Assert.Equal("x", secondInnerArg.Value);
        }

        [Fact]
        public void ThisMethodCallWithMultipleArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.compute(x, y + 10, false);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallStatement>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("compute", methodCall.Name);
            Assert.Equal(3, methodCall.Arguments.Count);

            var firstArg = Assert.IsType<IdentifiedNode>(methodCall.Arguments[0]);
            Assert.Equal("x", firstArg.Value);

            var secondArg = Assert.IsType<BinaryExpression>(methodCall.Arguments[1]);
            Assert.Equal(OperatorType.Add, secondArg.OperatorType);

            var thirdArg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[2]);
            Assert.False(thirdArg.Value);
        }
    }
}
