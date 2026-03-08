using JavaWhoCompiler;

namespace CompilerTests
{
    public class ParserTests
    {
        public static IEnumerable<object[]> BinaryExpressionData()
        {
            yield return new object[] {
                "a < 5;",
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.LessThan,
                new IntLiteral(5, new Position(0, 0))
            };

            yield return new object[] {
                "a == 5;",
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.Equal,
                new IntLiteral(5, new Position(0, 0))
            };

            yield return new object[] {
                "a + 5;",
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.Add,
                new IntLiteral(5, new Position(0, 0))
            };

            yield return new object[] {
                "a - 5;",
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.Subtract,
                new IntLiteral(5, new Position(0, 0))
            };

            yield return new object[] {
                "5 == true;",
                new IntLiteral(5, new Position(0, 0)),
                OperatorType.Equal,
                new BooleanLiteral(true, new Position(0, 0))
            };

            yield return new object[] {
                "5 != false;",
                new IntLiteral(5, new Position(0, 0)),
                OperatorType.NotEqual,
                new BooleanLiteral(false, new Position(0, 0))
            };

            yield return new object[] {
                "a * 5;",
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.Multiply,
                new IntLiteral(5, new Position(0, 0))
            };

            yield return new object[] {
                "10 / b;",
                new IntLiteral(10, new Position(0, 0)),
                OperatorType.Divide,
                new IdentifiedNode("b", new Position(0, 0))
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
        [Trait("Category", "Expression")]
        public void EmptyTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);
            Assert.Empty(program.Classes);
            Assert.Empty(program.Statements);
        }

        [Theory]
        [Trait("Category", "Expression")]
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

            Assert.True(Parser.ASTsEqual(expectedLeft, binaryExpression.Left));
            Assert.True(Parser.ASTsEqual(expectedRight, binaryExpression.Right));
            Assert.Equal(expectedOperator, binaryExpression.OperatorType);
        }

        [Theory]
        [Trait("Category", "ParserError")]
        [MemberData(nameof(NoSemicolonEndStmts))]
        public void ThrowOnNoSemicolonEndStmtTest(string code)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Theory]
        [Trait("Category", "ParserError")]
        [MemberData(nameof(NoSemicolonMidStmts))]
        public void ThrowOnNoSemicolonMidStmtTest(string code)
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize(code);
            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtNoArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println();");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);
            Assert.Null(println.Argument);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtIdentifierArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println(x);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);
            Assert.IsType<IdentifiedNode>(println.Argument);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtIntArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println(5);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);

            var intLiteral = Assert.IsType<IntLiteral>(println.Argument);
            Assert.Equal(5, intLiteral.Value);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtTrueArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println(true);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);

            var boolLiteral = Assert.IsType<BooleanLiteral>(println.Argument);
            Assert.True(boolLiteral.Value);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtFalseArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println(false);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);

            var boolLiteral = Assert.IsType<BooleanLiteral>(println.Argument);
            Assert.False(boolLiteral.Value);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void PrintLnStmtExpressionArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("println(x + 5);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var println = Assert.IsType<PrintLnStatement>(expressionStmt.Expression);

            Assert.Equal("println", println.Name);
            Assert.Null(println.Target);

            var expressionArg = Assert.IsType<BinaryExpression>(println.Argument);
            Assert.Equal(OperatorType.Add, expressionArg.OperatorType);
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void VardecStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Int x;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var vardecStatement = Assert.IsType<VariableDeclaration>(program.Statements[0]);

            var expected = new VariableDeclaration(
                    new IdentifiedNode("Int", new Position(0, 0)),
                    new IdentifiedNode("x", new Position(0, 0))
                    , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, vardecStatement));
        }

        [Fact]
        [Trait("Category", "Statement")]
        public void AssignStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("x = 5;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var assignStatement = Assert.IsType<AssignmentStatement>(program.Statements[0]);

            var expected = new AssignmentStatement(
                    new IdentifiedNode("x", new Position(0, 0)),
                    new IntLiteral(5, new Position(0, 0))
                    , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, assignStatement));
        }

        [Fact]
        [Trait("Category", "While")]
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
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                    , new Position(0, 0)),
                new AssignmentStatement(
                    new IdentifiedNode("x", new Position(0, 0)),
                    new BinaryExpression(
                        new IdentifiedNode("x", new Position(0, 0)),
                        OperatorType.Add,
                        new IntLiteral(1, new Position(0, 0))
                        , new Position(0, 0))
                    , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, whileStmt));
        }

        [Fact]
        [Trait("Category", "Statement")]
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
        [Trait("Category", "Statement")]
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
        [Trait("Category", "Statement")]
        public void ReturnStmtTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("return 5 + 8;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Empty(program.Classes);
            Assert.Single(program.Statements);

            var returnStmt = Assert.IsType<ReturnStatement>(program.Statements[0]);

            var expectedVal = new BinaryExpression(
                    new IntLiteral(5, new Position(0, 0)),
                    OperatorType.Add,
                    new IntLiteral(8, new Position(0, 0))
                    , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expectedVal, returnStmt.Val));
        }

        [Fact]
        [Trait("Category", "If")]
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
                        new IdentifiedNode("x", new Position(0, 0)),
                        OperatorType.NotEqual,
                        new IntLiteral(5, new Position(0, 0))
                        , new Position(0, 0)),
                    new ReturnStatement(
                        new IdentifiedNode("v", new Position(0, 0))
                    , new Position(0, 0)),
                    null
                , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, ifStatement));
        }

        [Fact]
        [Trait("Category", "If")]
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
                        new IdentifiedNode("x", new Position(0, 0)),
                        OperatorType.NotEqual,
                        new IntLiteral(5, new Position(0, 0))
                        , new Position(0, 0)),
                    new ReturnStatement(
                        new IdentifiedNode("v", new Position(0, 0))
                    , new Position(0, 0)),
                    new ReturnStatement(
                        new IdentifiedNode("x", new Position(0, 0))
                        , new Position(0, 0))
                , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, ifStatement));
        }

        [Fact]
        [Trait("Category", "Statement")]
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
        [Trait("Category", "Statement")]
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
                        new VariableDeclaration(
                                new IdentifiedNode("Int", new Position(0, 0)),
                                new IdentifiedNode("x", new Position(0, 0))
                                , new Position(0, 0)),
                        new AssignmentStatement(
                            new IdentifiedNode("x", new Position(0, 0)),
                            new BinaryExpression(
                                new IntLiteral(5, new Position(0, 0)),
                                OperatorType.Add,
                                new IntLiteral(2, new Position(0, 0))
                            , new Position(0, 0))
                        , new Position(0, 0))
            ];

            Parser.ASTListsEqual(expectedStmts, blockStatement.Statements);
        }

        [Fact]
        [Trait("Category", "Statement")]
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
                        new VariableDeclaration(
                                new IdentifiedNode("Int", new Position(0, 0)),
                                new IdentifiedNode("x", new Position(0, 0))
                                , new Position(0, 0)),
                        new AssignmentStatement(
                            new IdentifiedNode("x", new Position(0, 0)),
                            new BinaryExpression(
                                new IntLiteral(5, new Position(0, 0)),
                                OperatorType.Add,
                                new IntLiteral(2, new Position(0, 0))
                            , new Position(0, 0))
                        , new Position(0, 0)),
            ];

            Assert.True(Parser.ASTListsEqual(expected, program.Statements));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void OperatorPrecedenceMultiplyBeforeAddTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("2 + 3 * 4;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new IntLiteral(2, new Position(0, 0)),
                OperatorType.Add,
                new BinaryExpression(
                    new IntLiteral(3, new Position(0, 0)),
                    OperatorType.Multiply,
                    new IntLiteral(4, new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void OperatorPrecedenceAddBeforeCompareTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a + 2 < b + 3;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a", new Position(0, 0)),
                    OperatorType.Add,
                    new IntLiteral(2, new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.LessThan,
                new BinaryExpression(
                    new IdentifiedNode("b", new Position(0, 0)),
                    OperatorType.Add,
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void OperatorPrecedenceCompareBeforeEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a < 5 == b < 10;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.Equal,
                new BinaryExpression(
                    new IdentifiedNode("b", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void ComplexOperatorPrecedenceTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a * 2 + b / 3 - 4;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifiedNode("a", new Position(0, 0)),
                        OperatorType.Multiply,
                        new IntLiteral(2, new Position(0, 0))
                    , new Position(0, 0)),
                    OperatorType.Add,
                    new BinaryExpression(
                        new IdentifiedNode("b", new Position(0, 0)),
                        OperatorType.Divide,
                        new IntLiteral(3, new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.Subtract,
                new IntLiteral(4, new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
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
        [Trait("Category", "Expression")]
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
        [Trait("Category", "Expression")]
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
        [Trait("Category", "Expression")]
        public void SimpleNewObjectExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("new MyClass(x, y);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            NewObjectExpression newObjectExpression = Assert.IsType<NewObjectExpression>(expStmt.Expression);

            Assert.True(Parser.ASTsEqual(new IdentifiedNode("MyClass", new Position(0, 0)), newObjectExpression.ClassName));
            Assert.Equal(2, newObjectExpression.Arguments.Count);
            Assert.True(Parser.ASTsEqual(new IdentifiedNode("x", new Position(0, 0)), newObjectExpression.Arguments[0]));
            Assert.True(Parser.ASTsEqual(new IdentifiedNode("y", new Position(0, 0)), newObjectExpression.Arguments[1]));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void ComplexNewObjectExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("new MyClass(new OtherClass(z), y + x < 2);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            NewObjectExpression newObjectExpression = Assert.IsType<NewObjectExpression>(expStmt.Expression);

            Assert.True(Parser.ASTsEqual(new IdentifiedNode("MyClass", new Position(0, 0)), newObjectExpression.ClassName));
            Assert.Equal(2, newObjectExpression.Arguments.Count);

            NewObjectExpression nestedNewObject = Assert.IsType<NewObjectExpression>(newObjectExpression.Arguments[0]);
            
            Assert.True(Parser.ASTsEqual(new IdentifiedNode("OtherClass", new Position(0, 0)), nestedNewObject.ClassName));
            Assert.Single(nestedNewObject.Arguments);
            Assert.True(Parser.ASTsEqual(new IdentifiedNode("z", new Position(0, 0)), nestedNewObject.Arguments[0]));

            Assert.True(Parser.ASTsEqual(new BinaryExpression(
                        new BinaryExpression(
                            new IdentifiedNode("y", new Position(0, 0)),
                            OperatorType.Add,
                            new IdentifiedNode("x", new Position(0, 0))
                            , new Position(0, 0)),
                        OperatorType.LessThan,
                        new IntLiteral(2, new Position(0, 0))
                        , new Position(0, 0)),
                    newObjectExpression.Arguments[1]));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void SimpleParenthesizedLiteralTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(42);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            IntLiteral literal = Assert.IsType<IntLiteral>(expStmt.Expression);

            Assert.Equal(42, literal.Value);
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void ParenthesesOverridePrecedenceTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(2 + 3) * 4;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IntLiteral(2, new Position(0, 0)),
                    OperatorType.Add,
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.Multiply,
                new IntLiteral(4, new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void NestedParenthesesInExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(e + (a + b) * c) / d;");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("e", new Position(0, 0)),
                    OperatorType.Add,
                    new BinaryExpression(
                        new BinaryExpression(
                            new IdentifiedNode("a", new Position(0, 0)),
                            OperatorType.Add,
                            new IdentifiedNode("b", new Position(0, 0))
                        , new Position(0, 0)),
                        OperatorType.Multiply,
                        new IdentifiedNode("c", new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.Divide,
                new IdentifiedNode("d", new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void MultipleParenthesizedGroupsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(a + b) * (c - d);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a", new Position(0, 0)),
                    OperatorType.Add,
                    new IdentifiedNode("b", new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.Multiply,
                new BinaryExpression(
                    new IdentifiedNode("c", new Position(0, 0)),
                    OperatorType.Subtract,
                    new IdentifiedNode("d", new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void DeeplyNestedParenthesesTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(x + (y * (z + 1)));");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new IdentifiedNode("x", new Position(0, 0)),
                OperatorType.Add,
                new BinaryExpression(
                    new IdentifiedNode("y", new Position(0, 0)),
                    OperatorType.Multiply,
                    new BinaryExpression(
                        new IdentifiedNode("z", new Position(0, 0)),
                        OperatorType.Add,
                        new IntLiteral(1, new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void ParenthesesInComparisonTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("(a + b) < (c + d);");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new BinaryExpression(
                    new IdentifiedNode("a", new Position(0, 0)),
                    OperatorType.Add,
                    new IdentifiedNode("b", new Position(0, 0))
                , new Position(0, 0)),
                OperatorType.LessThan,
                new BinaryExpression(
                    new IdentifiedNode("c", new Position(0, 0)),
                    OperatorType.Add,
                    new IdentifiedNode("d", new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void ComplexNestedParenthesesWithDivisionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a * ((b + c) / (d - e));");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new IdentifiedNode("a", new Position(0, 0)),
                OperatorType.Multiply,
                new BinaryExpression(
                    new BinaryExpression(
                        new IdentifiedNode("b", new Position(0, 0)),
                        OperatorType.Add,
                        new IdentifiedNode("c", new Position(0, 0))
                    , new Position(0, 0)),
                    OperatorType.Divide,
                    new BinaryExpression(
                        new IdentifiedNode("d", new Position(0, 0)),
                        OperatorType.Subtract,
                        new IdentifiedNode("e", new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }

        [Fact]
        [Trait("Category", "Expression")]
        public void DoubleParenthesesTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("((x + 5));");

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            ExpressionStatement expStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var expected = new BinaryExpression(
                new IdentifiedNode("x", new Position(0, 0)),
                OperatorType.Add,
                new IntLiteral(5, new Position(0, 0))
            , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expected, expStmt.Expression));
        }
        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                outerIf.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("y", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0)),
                innerIf.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("x", new Position(0, 0)),
                    new IntLiteral(1, new Position(0, 0))
                , new Position(0, 0)),
                innerIf.IfBody
            ));
        }

        [Fact]
        [Trait("Category", "While")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                outerWhile.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("y", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0)),
                innerWhile.Guard
            ));

            var innerWhileBody = Assert.IsType<AssignmentStatement>(innerWhile.Statement);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new BinaryExpression(
                        new IdentifiedNode("y", new Position(0, 0)),
                        OperatorType.Add,
                        new IntLiteral(1, new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0)),
                innerWhileBody
            ));
        }

        [Fact]
        [Trait("Category", "While")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                whileStmt.Guard
            ));

            var block = Assert.IsType<BlockStatement>(whileStmt.Statement);

            Assert.Equal(2, block.Statements.Count);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("x", new Position(0, 0)),
                    new BinaryExpression(
                        new IdentifiedNode("x", new Position(0, 0)),
                        OperatorType.Add,
                        new IntLiteral(1, new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0)),
                block.Statements[0]
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new BinaryExpression(
                        new IdentifiedNode("y", new Position(0, 0)),
                        OperatorType.Add,
                        new IntLiteral(2, new Position(0, 0))
                    , new Position(0, 0))
                , new Position(0, 0)),
                block.Statements[1]
            ));
        }

        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.Equal,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                ifStmt.Guard
            ));

            var block = Assert.IsType<BlockStatement>(ifStmt.IfBody);

            Assert.Equal(2, block.Statements.Count);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("x", new Position(0, 0)),
                    new IntLiteral(0, new Position(0, 0))
                , new Position(0, 0)),
                block.Statements[0]
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new IntLiteral(0, new Position(0, 0))
                , new Position(0, 0)),
                block.Statements[1]
            ));

            Assert.Null(ifStmt.ElseBody);
        }

        [Fact]
        [Trait("Category", "If")]
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
        [Trait("Category", "If")]
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

            var expectedGuard = new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0));
            Assert.True(Parser.ASTsEqual(expectedGuard, outerIf.Guard));

            var expectedIfBody = new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new IntLiteral(1, new Position(0, 0))
                , new Position(0, 0));
            Assert.True(Parser.ASTsEqual(expectedIfBody, outerIf.IfBody));

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);

            expectedGuard = new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0));
            Assert.True(Parser.ASTsEqual(expectedGuard, elseIfStmt.Guard));

            expectedIfBody = new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new IntLiteral(2, new Position(0, 0))
                , new Position(0, 0));
            Assert.True(Parser.ASTsEqual(expectedIfBody, elseIfStmt.IfBody));

            expectedIfBody = new AssignmentStatement(
                    new IdentifiedNode("y", new Position(0, 0)),
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0));
            Assert.True(Parser.ASTsEqual(expectedIfBody, elseIfStmt.ElseBody));
        }

        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(new IdentifiedNode("x", new Position(0, 0)), OperatorType.Equal, new IntLiteral(1, new Position(0, 0)), new Position(0, 0)),
                firstIf.Guard
            ));
            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(new IdentifiedNode("x", new Position(0, 0)), OperatorType.Equal, new IntLiteral(2, new Position(0, 0)), new Position(0, 0)),
                secondIf.Guard
            ));
            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(new IdentifiedNode("x", new Position(0, 0)), OperatorType.Equal, new IntLiteral(3, new Position(0, 0)), new Position(0, 0)),
                thirdIf.Guard
            ));
            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(new IdentifiedNode("x", new Position(0, 0)), OperatorType.Equal, new IntLiteral(4, new Position(0, 0)), new Position(0, 0)),
                fourthIf.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(10, new Position(0, 0)), new Position(0, 0)),
                firstIf.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(20, new Position(0, 0)), new Position(0, 0)),
                secondIf.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(30, new Position(0, 0)), new Position(0, 0)),
                thirdIf.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(40, new Position(0, 0)), new Position(0, 0)),
                fourthIf.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(50, new Position(0, 0)), new Position(0, 0)),
                fourthIf.ElseBody
            ));
        }

        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(1, new Position(0, 0)), new Position(0, 0)),
                ifBlock.Statements[0]
            ));
            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("z", new Position(0, 0)), new IntLiteral(1, new Position(0, 0)), new Position(0, 0)),
                ifBlock.Statements[1]
            ));

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);
            var elseIfBlock = Assert.IsType<BlockStatement>(elseIfStmt.IfBody);

            Assert.Equal(2, elseIfBlock.Statements.Count);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(2, new Position(0, 0)), new Position(0, 0)),
                elseIfBlock.Statements[0]
            ));
            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("z", new Position(0, 0)), new IntLiteral(2, new Position(0, 0)), new Position(0, 0)),
                elseIfBlock.Statements[1]
            ));

            var finalElseBlock = Assert.IsType<BlockStatement>(elseIfStmt.ElseBody);

            Assert.Equal(2, finalElseBlock.Statements.Count);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(3, new Position(0, 0)), new Position(0, 0)),
                finalElseBlock.Statements[0]
            ));
            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("z", new Position(0, 0)), new IntLiteral(3, new Position(0, 0)), new Position(0, 0)),
                finalElseBlock.Statements[1]
            ));
        }

        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(1, new Position(0, 0)), new Position(0, 0)),
                outerIf.IfBody
            ));

            var elseBlock = Assert.IsType<BlockStatement>(outerIf.ElseBody);

            Assert.Single(elseBlock.Statements);

            var nestedIf = Assert.IsType<IfStatement>(elseBlock.Statements[0]);

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("z", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0)),
                nestedIf.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(2, new Position(0, 0)), new Position(0, 0)),
                nestedIf.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(new IdentifiedNode("y", new Position(0, 0)), new IntLiteral(3, new Position(0, 0)), new Position(0, 0)),
                nestedIf.ElseBody
            ));
        }

        [Fact]
        [Trait("Category", "If")]
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

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(5, new Position(0, 0))
                , new Position(0, 0)),
                outerIf.Guard
            ));

            var ifBlock = Assert.IsType<BlockStatement>(outerIf.IfBody);

            Assert.Single(ifBlock.Statements);

            var nestedIfInIfBlock = Assert.IsType<IfStatement>(ifBlock.Statements[0]);

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("y", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0)),
                nestedIfInIfBlock.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("z", new Position(0, 0)),
                    new IntLiteral(1, new Position(0, 0))
                , new Position(0, 0)),
                nestedIfInIfBlock.IfBody
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("z", new Position(0, 0)),
                    new IntLiteral(2, new Position(0, 0))
                , new Position(0, 0)),
                nestedIfInIfBlock.ElseBody
            ));

            var elseIfStmt = Assert.IsType<IfStatement>(outerIf.ElseBody);

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("x", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(10, new Position(0, 0))
                , new Position(0, 0)),
                elseIfStmt.Guard
            ));

            var elseIfBlock = Assert.IsType<BlockStatement>(elseIfStmt.IfBody);

            Assert.Single(elseIfBlock.Statements);

            var nestedIfInElseIfBlock = Assert.IsType<IfStatement>(elseIfBlock.Statements[0]);

            Assert.True(Parser.ASTsEqual(
                new BinaryExpression(
                    new IdentifiedNode("y", new Position(0, 0)),
                    OperatorType.LessThan,
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0)),
                nestedIfInElseIfBlock.Guard
            ));

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("z", new Position(0, 0)),
                    new IntLiteral(3, new Position(0, 0))
                , new Position(0, 0)),
                nestedIfInElseIfBlock.IfBody
            ));

            Assert.Null(nestedIfInElseIfBlock.ElseBody);

            var finalElseBlock = Assert.IsType<BlockStatement>(elseIfStmt.ElseBody);

            Assert.Single(finalElseBlock.Statements);

            Assert.True(Parser.ASTsEqual(
                new AssignmentStatement(
                    new IdentifiedNode("z", new Position(0, 0)),
                    new IntLiteral(4, new Position(0, 0))
                , new Position(0, 0)),
                finalElseBlock.Statements[0]
            ));
        }

        [Fact]
        [Trait("Category", "Method")]
        public void SimpleMethodCallExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.run();");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("run", methodCall.Name);
            Assert.Empty(methodCall.Arguments);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void SimpleChainedMethodCallExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("a.run().jump().walk();");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);

            var finalMethodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);
            var innerMethodCall = Assert.IsType<MethodCallExpression>(finalMethodCall.Target);
            var outterMethodCall = Assert.IsType<MethodCallExpression>(innerMethodCall.Target);

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
        [Trait("Category", "Method")]
        public void MethodCallWithSingleIntArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(42);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Statements);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            var target = Assert.IsType<IdentifiedNode>(methodCall.Target);
            Assert.Equal("obj", target.Value);
            Assert.Equal("calculate", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<IntLiteral>(methodCall.Arguments[0]);
            Assert.Equal(42, arg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithSingleBooleanArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.setFlag(true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("setFlag", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[0]);
            Assert.True(arg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithSingleStringArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.print(\"hello\");");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("print", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<StringLiteral>(methodCall.Arguments[0]);
            Assert.Equal("\"hello\"", arg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithSingleIdentifierArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.process(x);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.IsType<ThisExpression>(methodCall.Target);
            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<IdentifiedNode>(methodCall.Arguments[0]);
            Assert.Equal("x", arg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithMultipleArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(5, x, true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

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
        [Trait("Category", "Method")]
        public void MethodCallWithBinaryExpressionArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(x + 5);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

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
        [Trait("Category", "Method")]
        public void MethodCallWithComplexBinaryExpressionArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.evaluate(a * 2 + b);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("evaluate", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var arg = Assert.IsType<BinaryExpression>(methodCall.Arguments[0]);
            Assert.Equal(OperatorType.Add, arg.OperatorType);

            var leftSide = Assert.IsType<BinaryExpression>(arg.Left);
            Assert.Equal(OperatorType.Multiply, leftSide.OperatorType);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithNestedMethodCallArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.process(helper.getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var nestedCall = Assert.IsType<MethodCallExpression>(methodCall.Arguments[0]);
            Assert.Equal("getValue", nestedCall.Name);
            Assert.Empty(nestedCall.Arguments);

            var nestedTarget = Assert.IsType<IdentifiedNode>(nestedCall.Target);
            Assert.Equal("helper", nestedTarget.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithMultipleNestedMethodCallArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.compare(a.getValue(), b.getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("compare", methodCall.Name);
            Assert.Equal(2, methodCall.Arguments.Count);

            var firstNestedCall = Assert.IsType<MethodCallExpression>(methodCall.Arguments[0]);
            Assert.Equal("getValue", firstNestedCall.Name);
            var firstTarget = Assert.IsType<IdentifiedNode>(firstNestedCall.Target);
            Assert.Equal("a", firstTarget.Value);

            var secondNestedCall = Assert.IsType<MethodCallExpression>(methodCall.Arguments[1]);
            Assert.Equal("getValue", secondNestedCall.Name);
            var secondTarget = Assert.IsType<IdentifiedNode>(secondNestedCall.Target);
            Assert.Equal("b", secondTarget.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithChainedMethodCallArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.process(helper.getManager().getValue());");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("process", methodCall.Name);
            Assert.Single(methodCall.Arguments);

            var nestedCall = Assert.IsType<MethodCallExpression>(methodCall.Arguments[0]);
            Assert.Equal("getValue", nestedCall.Name);

            var innerCall = Assert.IsType<MethodCallExpression>(nestedCall.Target);
            Assert.Equal("getManager", innerCall.Name);

            var innerTarget = Assert.IsType<IdentifiedNode>(innerCall.Target);
            Assert.Equal("helper", innerTarget.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithMixedComplexArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.calculate(x + 5, helper.getValue(), true, 42);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("calculate", methodCall.Name);
            Assert.Equal(4, methodCall.Arguments.Count);

            var firstArg = Assert.IsType<BinaryExpression>(methodCall.Arguments[0]);
            Assert.Equal(OperatorType.Add, firstArg.OperatorType);

            var secondArg = Assert.IsType<MethodCallExpression>(methodCall.Arguments[1]);
            Assert.Equal("getValue", secondArg.Name);

            var thirdArg = Assert.IsType<BooleanLiteral>(methodCall.Arguments[2]);
            Assert.True(thirdArg.Value);

            var fourthArg = Assert.IsType<IntLiteral>(methodCall.Arguments[3]);
            Assert.Equal(42, fourthArg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void ChainedMethodCallWithArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.first(5).second(x).third(true);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var finalCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("third", finalCall.Name);
            Assert.Single(finalCall.Arguments);
            var finalArg = Assert.IsType<BooleanLiteral>(finalCall.Arguments[0]);
            Assert.True(finalArg.Value);

            var middleCall = Assert.IsType<MethodCallExpression>(finalCall.Target);
            Assert.Equal("second", middleCall.Name);
            Assert.Single(middleCall.Arguments);
            var middleArg = Assert.IsType<IdentifiedNode>(middleCall.Arguments[0]);
            Assert.Equal("x", middleArg.Value);

            var firstCall = Assert.IsType<MethodCallExpression>(middleCall.Target);
            Assert.Equal("first", firstCall.Name);
            Assert.Single(firstCall.Arguments);
            var firstArg = Assert.IsType<IntLiteral>(firstCall.Arguments[0]);
            Assert.Equal(5, firstArg.Value);

            var target = Assert.IsType<IdentifiedNode>(firstCall.Target);
            Assert.Equal("obj", target.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void MethodCallWithMethodCallWithArgumentsAsArgumentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("obj.outer(inner.compute(5, x));");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var outerCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

            Assert.Equal("outer", outerCall.Name);
            Assert.Single(outerCall.Arguments);

            var innerCall = Assert.IsType<MethodCallExpression>(outerCall.Arguments[0]);
            Assert.Equal("compute", innerCall.Name);
            Assert.Equal(2, innerCall.Arguments.Count);

            var firstInnerArg = Assert.IsType<IntLiteral>(innerCall.Arguments[0]);
            Assert.Equal(5, firstInnerArg.Value);

            var secondInnerArg = Assert.IsType<IdentifiedNode>(innerCall.Arguments[1]);
            Assert.Equal("x", secondInnerArg.Value);
        }

        [Fact]
        [Trait("Category", "Method")]
        public void ThisMethodCallWithMultipleArgumentsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("this.compute(x, y + 10, false);");

            AST root = Parser.Parse(tokens);

            var program = Assert.IsType<ProgramNode>(root);

            var expressionStmt = Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var methodCall = Assert.IsType<MethodCallExpression>(expressionStmt.Expression);

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

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass {
                        init() {}
                        method test(Int x, String y) Void {
                            return;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        null,
                        [],
                        new Constructor(
                            [],
                            null,
                            []
                            , new Position(0, 0)),
                        [
                            new MethodDefinition(
                                new IdentifiedNode("test", new Position(0, 0)),
                                [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                                ],
                                null,
                                new BlockStatement([
                                    new ReturnStatement(null, new Position(0, 0))
                                ], new Position(0, 0))
                            , new Position(0, 0))
                        ]
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefVardecTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass {
                        Int z;
                        String a;
                        init() {}
                        method test(Int x, String y) Void {
                            return;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        null,
                        [
                            new VariableDeclaration(
                                new IdentifiedNode("Int", new Position(0, 0)),
                                new IdentifiedNode("z", new Position(0, 0))
                                , new Position(0, 0)),
                            new VariableDeclaration(
                                new IdentifiedNode("String", new Position(0, 0)),
                                new IdentifiedNode("a", new Position(0, 0))
                                , new Position(0, 0))
                        ],
                        new Constructor(
                            [],
                            null,
                            []
                            , new Position(0, 0)),
                        [
                            new MethodDefinition(
                                new IdentifiedNode("test", new Position(0, 0)),
                                [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                                ],
                                null,
                                new BlockStatement([
                                    new ReturnStatement(null, new Position(0, 0))
                                ], new Position(0, 0))
                            , new Position(0, 0))
                        ]
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassVardecNoSemicolonTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        Int a

                        init(Int x, String y) {}

                        method test1(Int x, String y) {
                            return;
                        }
                    }
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefExtendTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init() {}
                        method test(Int x, String y) Void {
                            return;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        new IdentifiedNode("OtherClass", new Position(0, 0)),
                        [],
                        new Constructor(
                            [],
                            null,
                            []
                            , new Position(0, 0)),
                        [
                            new MethodDefinition(
                                new IdentifiedNode("test", new Position(0, 0)),
                                [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                                ],
                                null,
                                new BlockStatement([
                                    new ReturnStatement(null, new Position(0, 0))
                                ], new Position(0, 0))
                            , new Position(0, 0))
                        ]
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefMultMethodsTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init() {}
                        method test1(Int x, String y) Void {
                            return;
                        }

                        method test2(Int x, String y) Boolean {
                            return true;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        new IdentifiedNode("OtherClass", new Position(0, 0)),
                        [],
                        new Constructor(
                            [],
                            null,
                            []
                            , new Position(0, 0)),
                        [
                            new MethodDefinition(
                                new IdentifiedNode("test1", new Position(0, 0)),
                                [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                                ],
                                null,
                                new BlockStatement([
                                    new ReturnStatement(null, new Position(0, 0))
                                ], new Position(0, 0))
                            , new Position(0, 0)),
                            new MethodDefinition(
                                new IdentifiedNode("test2", new Position(0, 0)),
                                [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                                ],
                                new IdentifiedNode("Boolean", new Position(0, 0)),
                                new BlockStatement([
                                    new ReturnStatement(
                                        new BooleanLiteral(true, new Position(0, 0))
                                        , new Position(0, 0))
                                ], new Position(0, 0))
                            , new Position(0, 0))
                        ]
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefMethodNoRetTypeTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init(Int x, String y) {}

                        method test1(Int x, String y) {
                            return;
                        }
                    }
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefConstructorNoSuperTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            Int z;
                            z = x;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        new IdentifiedNode("OtherClass", new Position(0, 0)),
                        [],
                        new Constructor(
                            [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                            ],
                            null,
                            [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("z", new Position(0, 0))
                                    , new Position(0, 0)),
                                new AssignmentStatement(
                                    new IdentifiedNode("z", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0))
                            ]
                            , new Position(0, 0)),
                        []
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefConstructorSuperTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            super(x, y);
                            Int z;
                            z = x;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Single(program.Classes);
            Assert.Empty(program.Statements);


            var expClassDef = new ClassDefinition(
                        new IdentifiedNode("MyClass", new Position(0, 0)),
                        new IdentifiedNode("OtherClass", new Position(0, 0)),
                        [],
                        new Constructor(
                            [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0)),
                                new VariableDeclaration(
                                    new IdentifiedNode("String", new Position(0, 0)),
                                    new IdentifiedNode("y", new Position(0, 0))
                                    , new Position(0, 0)),
                            ],
                            [
                                new IdentifiedNode("x", new Position(0, 0)),
                                new IdentifiedNode("y", new Position(0, 0))
                            ],
                            [
                                new VariableDeclaration(
                                    new IdentifiedNode("Int", new Position(0, 0)),
                                    new IdentifiedNode("z", new Position(0, 0))
                                    , new Position(0, 0)),
                                new AssignmentStatement(
                                    new IdentifiedNode("z", new Position(0, 0)),
                                    new IdentifiedNode("x", new Position(0, 0))
                                    , new Position(0, 0))
                            ]
                            , new Position(0, 0)),
                        []
                        , new Position(0, 0));

            Assert.True(Parser.ASTsEqual(expClassDef, program.Classes[0]));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefConstructorBadSuperTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            Int z;
                            z = x;

                            super(x, y);
                        }
                    }
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefMultConstructorTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            Int z;
                            z = x;
                        }
                        init(Int a, String y) {
                            Int z;
                            z = x;
                        }
                    }
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ClassDefOutOfOrderTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyClass extends OtherClass {
                        method b(String s) String {
                            return s;
                        }

                        init(Int x, String y) {
                            Int z;
                            z = x;
                        }

                        Int a;
                    }
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));
        }

        [Fact]
        [Trait("Category", "Class")]
        public void MultipleClassesCountTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class OtherClass {
                        init() {}
                    }

                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            super(x, y);

                            Int z;
                            z = x;
                        }
                    }
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Equal(2, program.Classes.Count);
            Assert.Empty(program.Statements);
        }

        [Fact]
        [Trait("Category", "Class")]
        public void MultipleClassesAndStatementsCountTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class OtherClass {
                        init() {}
                    }

                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            super(x, y);

                            Int z;
                            z = x;
                        }
                    }

                    OtherClass c;
                    String s;
                    s = "test string";
                    println(s);
                    """);

            AST root = Parser.Parse(tokens);

            ProgramNode program = Assert.IsType<ProgramNode>(root);

            Assert.Equal(2, program.Classes.Count);
            Assert.Equal(4, program.Statements.Count);
        }

        [Fact]
        [Trait("Category", "Class")]
        public void CantMisplaceClassAndStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class OtherClass {
                        init() {}
                    }

                    OtherClass c;
                    String s;

                    class MyClass extends OtherClass {
                        init(Int x, String y) {
                            super(x, y);

                            Int z;
                            z = x;
                        }
                    }

                    s = "test string";
                    println(s);
                    """);

            Assert.Throws<ParserException>(() => Parser.Parse(tokens));

        }
    }
}
