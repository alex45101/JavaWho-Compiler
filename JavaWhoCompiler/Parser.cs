namespace JavaWhoCompiler
{
    public enum OperatorType
    { 
        Add,
        Subtract,
        Multiply,
        Divide,
        LessThan,
        Equal,
        NotEqual
    }

    public abstract record AST;
    public sealed record ProgramNode(List<AST> Classes, List<AST> Statements) : AST;

    public sealed record IntLiteral(int Value) : AST;
    public sealed record StringLiteral(string Value) : AST;
    public sealed record BooleanLiteral(bool Value) : AST;
    public sealed record IdentifiedNode(string Value) : AST;

    //expressions
    public sealed record ThisExpression() : AST;
    public sealed record PrimaryExpression(string Value) : AST;
    public sealed record BinaryExpression(AST Left, OperatorType OperatorType, AST Right) : AST;

    //statements
    public sealed record ExpressionStatement(AST Expression) : AST;
    public sealed record VariableDeclarationStatement(IdentifiedNode Type, IdentifiedNode Var) : AST;
    public sealed record AssignmentStatement(IdentifiedNode Var, AST Val) : AST;
    public sealed record WhileStatement(AST Guard, AST Statement) : AST;
    public sealed record BreakStatement() : AST;
    public sealed record ReturnStatement(AST Val) : AST;
    public sealed record IfStatement(AST Guard, AST IfBody, AST ElseBody) : AST;
    public sealed record BlockStatement(List<AST> Statements) : AST;
    public sealed record MethodCallStatement(string Name, AST Target, List<AST> Arguments) : AST;


    public class ParserException(string message) : Exception(message);

    public class Parser
    {
        private IToken[] tokens = null;
        private int currPos = 0;

        private bool IsEnd => currPos >= tokens.Length;
        private IToken CurrentToken => !IsEnd ? tokens[currPos] : throw new IndexOutOfRangeException();
        private IToken Consume() => !IsEnd ? tokens[currPos++] : throw new IndexOutOfRangeException();
        private IToken GetTokenAt(int pos) => !IsEnd ? tokens[pos] : throw new IndexOutOfRangeException();
        private IToken PeekNext() => currPos + 1 < tokens.Length ? tokens[currPos + 1] : null;
        private bool Check<T>() where T : IToken => !IsEnd && CurrentToken is T;
        private IToken Expect<T>() where T : IToken
        {
            if (!Check<T>())
            {
                throw new ParserException($"Expected {typeof(T)} but current token is {CurrentToken.GetType()}");
            }

            return Consume();
        }
        private AST ConsumeAndReturn(AST node)
        {
            Consume();
            return node;
        }

        public static AST Parse(IEnumerable<IToken> tokens)
        {
            Parser parser = new Parser(tokens);
            List<AST> classes = new List<AST>();
            List<AST> statements = new List<AST>();

            //TODO: Parse classes

            while (!parser.IsEnd)
            {
                statements.Add(parser.ParseStatement());
            }
            
            return new ProgramNode(classes, statements);
        }

        private Parser(IEnumerable<IToken> tokens)
        {
            this.tokens = tokens.ToArray();
            currPos = 0;
        }

        private AST ParseStatement()
        {
            AST stmt = CurrentToken switch {
                WhileToken => ParseWhileStatement(),
                BreakToken => ParseBreakStatement(),
                ReturnToken => ParseReturnStatement(),
                IfToken => ParseIfStatement(),
                OpenCurlyBracketToken => ParseBlockStatement(),
                IdentifierToken => PeekNext() switch {
                    IdentifierToken => ParseVardecStatement(),
                    AssignmentOperatorToken => ParseAssignStatement(),

                    // no token ahead of identifier
                    null => throw new ParserException("Expected ';', got EOF"),

                    // try expression statement
                    _ => ParseExpressionStatement(),
                },

                // try expression statement
                _ => ParseExpressionStatement(),
            };

            return stmt;
        }

        private AST ParseWhileStatement() {
            Expect<WhileToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST body = ParseStatement();

            return new WhileStatement(guard, body);
        }

        private AST ParseBreakStatement() {
            Expect<BreakToken>();
            Expect<SemiColonToken>();

            return new BreakStatement();
        }

        private AST ParseReturnStatement() {
            Expect<ReturnToken>();


            AST val = null;

            if (!Check<SemiColonToken>())
            {
                val = ParseExpression();
            }

            Expect<SemiColonToken>();

            return new ReturnStatement(val);
        }

        private AST ParseIfStatement() {
            Expect<IfToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST ifBody = ParseStatement();

            AST elseBody = null;

            //no else token next then return right away with null elsebody
            if (!Check<ElseToken>())
            {
                return new IfStatement(guard, ifBody, elseBody);
            }

            Consume(); //eat else token

            elseBody = ParseStatement();

            return new IfStatement(guard, ifBody, elseBody);
        }

        private AST ParseBlockStatement() {
            Expect<OpenCurlyBracketToken>();

            List<AST> stmts = [];
            while(CurrentToken is not CloseCurlyBracketToken) {
                stmts.Add(ParseStatement());
            }

            // consume closing bracket
            Consume();

            return new BlockStatement(stmts);
        }

        private AST ParseVardecStatement() {
            
            string typeIdent = Expect<IdentifierToken>().Value;

            
            string varIdent = Expect<IdentifierToken>().Value;

            Expect<SemiColonToken>();
            
            return new VariableDeclarationStatement(
                    new IdentifiedNode(typeIdent), 
                    new IdentifiedNode(varIdent)
                    );
        }

        private AST ParseAssignStatement() {
            
            string varIdent = Expect<IdentifierToken>().Value;

            Expect<AssignmentOperatorToken>();
            
            AST value = ParseExpression();

            Expect<SemiColonToken>();

            return new AssignmentStatement(
                    new IdentifiedNode(varIdent), 
                    value
                    );
        }

        private AST ParseExpressionStatement() {
            AST exp = ParseExpression();

            Expect<SemiColonToken>();

            return new ExpressionStatement(exp);
        }

        private AST ParseExpression() => ParseEqualityExpression();

        private AST ParseEqualityExpression()
        {
            AST left = ParseCompareExpression();

            if (Check<EqualsOperatorToken>() || Check<NotEqualsOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is EqualsOperatorToken ? OperatorType.Equal : OperatorType.NotEqual;
                
                Consume();

                AST right = ParseCompareExpression();

                return new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseCompareExpression()
        {
            AST left = ParseAddExpression();

            if (Check<LessThanOperatorToken>())
            {
                OperatorType operatorType = OperatorType.LessThan;

                Consume();

                AST right = ParseAddExpression();

                return new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseAddExpression()
        {
            AST left = ParseMultiplyExpression();

            while (Check<AddOperatorToken>() || Check<SubtractOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is AddOperatorToken ? OperatorType.Add : OperatorType.Subtract;

                Consume();

                AST right = ParseMultiplyExpression();

                left = new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseMultiplyExpression()
        {
            AST left = ParseCallExpression();

            while (Check<MultiplyOperatorToken>() || Check<DivideOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is MultiplyOperatorToken ? OperatorType.Multiply : OperatorType.Divide;

                Consume();

                AST right = ParseCallExpression();

                left = new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseCallExpression()
        {
            AST left = ParsePrimaryExpression();

            while (Check<DotToken>())
            {
                Consume();

                left = MethodCallExpression(left);
            }

            return left;
        }

        private AST ParsePrimaryExpression()
        {
            AST primaryNode = CurrentToken switch
            {
                IdentifierToken => new IdentifiedNode(Consume().Value),
                StringToken => new StringLiteral(Consume().Value),
                NumberToken => new IntLiteral((Consume() as NumberToken).Number),
                TrueToken or FalseToken => new BooleanLiteral(Consume().Value == "true"),
                ThisToken => ConsumeAndReturn(new ThisExpression()),
                _ => throw new ParserException($"Unexpected token {CurrentToken.GetType().Name}")
            };

            return primaryNode;
        }

        private AST MethodCallExpression(AST Target)
        {
            var token = Expect<IdentifierToken>();
            Expect<OpenParenthesisToken>();

            List<AST> arguments = CommaExpression();

            Expect<CloseParenthesisToken>();

            return new MethodCallStatement(token.Value, Target, arguments);
        }

        private List<AST> CommaExpression()
        {
            List<AST> result = new List<AST>();

            while (!Check<CloseParenthesisToken>())
            {
                AST exp = ParseExpression();

                if (Check<CommaToken>())
                {
                    Consume();
                }

                result.Add(exp);
            }

            return result;
        }
    }
}
