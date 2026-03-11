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

    public abstract record AST(Position Position);
    public sealed record ProgramNode(List<AST> Classes, List<AST> Statements) : AST(new Position(1, 1));

    public sealed record IntLiteral(int Value, Position Position) : AST(Position);
    public sealed record StringLiteral(string Value, Position Position) : AST(Position);
    public sealed record BooleanLiteral(bool Value, Position Position) : AST(Position);
    public sealed record IdentifiedNode(string Value, Position Position) : AST(Position);

    //expressions
    public sealed record ThisExpression(Position Position) : AST(Position);
    public sealed record PrimaryExpression(string Value, Position Position) : AST(Position);
    public sealed record BinaryExpression(AST Left, OperatorType OperatorType, AST Right, Position Position) : AST(Position);
    public record MethodCallExpression(string Name, AST Target, List<AST> Arguments, Position Position) : AST(Position);
    public sealed record NewObjectExpression(IdentifiedNode ClassName, List<AST> Arguments, Position Position): AST(Position);

    //statements
    public sealed record ExpressionStatement(AST Expression, Position Position) : AST(Position);
    public sealed record VariableDeclaration(IdentifiedNode Type, IdentifiedNode Var, Position Position) : AST(Position);
    public sealed record AssignmentStatement(IdentifiedNode Var, AST Val, Position Position) : AST(Position);
    public sealed record WhileStatement(AST Guard, AST Statement, Position Position) : AST(Position);
    public sealed record BreakStatement(Position Position) : AST(Position);
    public sealed record ReturnStatement(AST Val, Position Position) : AST(Position);
    public sealed record IfStatement(AST Guard, AST IfBody, AST ElseBody, Position Position) : AST(Position);
    public sealed record BlockStatement(List<AST> Statements, Position Position) : AST(Position);
    public sealed record PrintLnStatement(AST Argument, Position Position) : MethodCallExpression("println", null, [Argument], Position);

    //class
    public sealed record MethodDefinition(IdentifiedNode Name, List<AST> Parameters, IdentifiedNode ReturnType, AST Body, Position Position) : AST(Position);
    public sealed record Constructor(List<AST> Parameters, List<AST> SuperArguments, List<AST> Statements, Position Position) : AST(Position);
    public sealed record ClassDefinition(IdentifiedNode Name, IdentifiedNode ExtendsName, List<AST> VariableDeclarations, AST Constructor, List<AST> MethodDefinitions, Position Position) : AST(Position);



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
            if(IsEnd) 
            {
                throw new ParserException($"Expected {typeof(T)} but reached end of file");
            } else if (!Check<T>())
            {
                throw new ParserException($"{CurrentToken.Position.Line}:{CurrentToken.Position.Column}: Expected {typeof(T)} but current token is {CurrentToken.GetType()}");
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

            while (parser.Check<ClassToken>())
            {
                classes.Add(parser.ParseClassDefinition());
            }

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
            AST stmt = CurrentToken switch
            {
                WhileToken => ParseWhileStatement(),
                BreakToken => ParseBreakStatement(),
                ReturnToken => ParseReturnStatement(),
                IfToken => ParseIfStatement(),
                OpenCurlyBracketToken => ParseBlockStatement(),
                IdentifierToken => PeekNext() switch
                {
                    IdentifierToken => ParseVariableDeclarationStatement(),
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

        private AST ParseWhileStatement()
        {
            var startToken = Expect<WhileToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST body = ParseStatement();

            return new WhileStatement(guard, body, startToken.Position);
        }

        private AST ParseBreakStatement()
        {
            var startToken = Expect<BreakToken>();
            Expect<SemiColonToken>();

            return new BreakStatement(startToken.Position);
        }

        private AST ParseReturnStatement()
        {
            var startToken = Expect<ReturnToken>();

            AST val = null;

            if (!Check<SemiColonToken>())
            {
                val = ParseExpression();
            }

            Expect<SemiColonToken>();

            return new ReturnStatement(val, startToken.Position);
        }

        private AST ParseIfStatement()
        {
            var startToken = Expect<IfToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST ifBody = ParseStatement();

            AST elseBody = null;

            //no else token next then return right away with null elsebody
            if (!Check<ElseToken>())
            {
                return new IfStatement(guard, ifBody, elseBody, startToken.Position);
            }

            Consume(); //eat else token

            elseBody = ParseStatement();

            return new IfStatement(guard, ifBody, elseBody, startToken.Position);
        }

        private AST ParseBlockStatement()
        {
            var startToken = Expect<OpenCurlyBracketToken>();

            List<AST> stmts = [];
            while (CurrentToken is not CloseCurlyBracketToken)
            {
                stmts.Add(ParseStatement());
            }

            // consume closing bracket
            Consume();

            return new BlockStatement(stmts, startToken.Position);
        }

        private AST ParseVariableDeclarationStatement()
        {

            var vardec = ParseVariableDeclaration();

            Expect<SemiColonToken>();

            return vardec;
        }

        private AST ParseAssignStatement()
        {

            var startToken = Expect<IdentifierToken>();

            Expect<AssignmentOperatorToken>();

            AST value = ParseExpression();

            Expect<SemiColonToken>();

            return new AssignmentStatement(
                    new IdentifiedNode(startToken.Value, startToken.Position),
                    value,
                    startToken.Position
                    );
        }

        private AST ParseExpressionStatement()
        {
            AST exp = ParseExpression();

            Expect<SemiColonToken>();

            return new ExpressionStatement(exp, exp.Position);
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

                return new BinaryExpression(left, operatorType, right, left.Position);
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

                return new BinaryExpression(left, operatorType, right, left.Position);
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

                left = new BinaryExpression(left, operatorType, right, left.Position);
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

                left = new BinaryExpression(left, operatorType, right, left.Position);
            }

            return left;
        }

        private AST ParseCallExpression()
        {
            if (Check<PrintLnToken>())
            {
                var startToken = Consume();
                Expect<OpenParenthesisToken>();

                List<AST> arguments = CommaExpression();

                Expect<CloseParenthesisToken>();

                if (arguments.Count > 1) throw new ParserException("Invalid amount of arguments for println");

                return new PrintLnStatement(arguments.FirstOrDefault(), startToken.Position);
            }

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
            if(IsEnd) {
                throw new ParserException("Expected primary expression but reached end of file");
            }

            AST primaryNode = CurrentToken switch
            {
                OpenParenthesisToken => ParseParenthesisExpression(),
                IdentifierToken(string value, Position position) => ConsumeAndReturn(new IdentifiedNode(value, position)),
                StringToken(string value, Position position) => ConsumeAndReturn(new StringLiteral(value, position)),
                NumberToken(_, Position position) => new IntLiteral((Consume() as NumberToken).Number, position),
                TrueToken(_, Position position) => ConsumeAndReturn(new BooleanLiteral(true, position)),
                FalseToken(_, Position position) => ConsumeAndReturn(new BooleanLiteral(false, position)),
                ThisToken(_, Position position) => ConsumeAndReturn(new ThisExpression(position)),
                NewToken => ParseNewObjectExpression(),
                _ => throw new ParserException($"Unexpected token {CurrentToken.GetType().Name}")
            };

            return primaryNode;
        }

        private AST ParseNewObjectExpression()
        {
            var startToken = Expect<NewToken>();

            string className = Expect<IdentifierToken>().Value;

            Expect<OpenParenthesisToken>();

            List<AST> args = CommaExpression();

            Expect<CloseParenthesisToken>();

            return new NewObjectExpression(new IdentifiedNode(className, startToken.Position), args, startToken.Position);
        }

        private AST ParseParenthesisExpression()
        {
            Consume();
            AST yeet = ParseExpression();

            Expect<CloseParenthesisToken>();

            return yeet;
        }

        private AST MethodCallExpression(AST Target)
        {
            var token = Expect<IdentifierToken>();
            Expect<OpenParenthesisToken>();

            List<AST> arguments = CommaExpression();

            Expect<CloseParenthesisToken>();

            return new MethodCallExpression(token.Value, Target, arguments, token.Position);
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

        private AST ParseVariableDeclaration()
        {
            var typeToken = Expect<IdentifierToken>();
            var varToken = Expect<IdentifierToken>();

            return new VariableDeclaration(
                    new IdentifiedNode(typeToken.Value, typeToken.Position),
                    new IdentifiedNode(varToken.Value, varToken.Position),
                    typeToken.Position
                    );
        }

        private List<AST> ParseCommaVariableDeclaration()
        {
            if (!Check<IdentifierToken>()) return [];

            List<AST> vardecs = [ParseVariableDeclaration()];

            while (Check<CommaToken>())
            {
                Consume();
                vardecs.Add(ParseVariableDeclaration());
            }

            return vardecs;
        }

        private AST ParseMethodDefinition()
        {
            var startToken = Expect<MethodToken>();

            var methodNameToken = Expect<IdentifierToken>();

            Expect<OpenParenthesisToken>();

            List<AST> parameters = ParseCommaVariableDeclaration();

            Expect<CloseParenthesisToken>();

            // check for void first
            IdentifiedNode returnType = null;
            if (Check<VoidTypeToken>())
            {
                Consume();
            }
            else
            {
                var retTypeToken = Expect<IdentifierToken>();
                returnType = new IdentifiedNode(
                                retTypeToken.Value,
                                retTypeToken.Position
                             );
            }

            AST body = ParseBlockStatement();

            return new MethodDefinition(
                    new IdentifiedNode(methodNameToken.Value, methodNameToken.Position),
                    parameters,
                    returnType,
                    body,
                    startToken.Position
                    );
        }

        private AST ParseConstructor()
        {
            var startToken = Expect<InitToken>();

            Expect<OpenParenthesisToken>();

            List<AST> parameters = ParseCommaVariableDeclaration();

            Expect<CloseParenthesisToken>();

            Expect<OpenCurlyBracketToken>();

            List<AST> superArgs = null;

            // optional super call
            if (Check<SuperToken>())
            {
                Consume();
                Expect<OpenParenthesisToken>();
                superArgs = CommaExpression();
                Expect<CloseParenthesisToken>();
                Expect<SemiColonToken>();
            }

            List<AST> statements = [];
            while (!Check<CloseCurlyBracketToken>())
            {
                statements.Add(ParseStatement());
            }

            Expect<CloseCurlyBracketToken>();

            return new Constructor(
                    parameters,
                    superArgs,
                    statements,
                    startToken.Position
                    );
        }

        private AST ParseClassDefinition()
        {
            var startToken = Expect<ClassToken>();

            var classNameToken = Expect<IdentifierToken>();

            IdentifiedNode extendsName = null;
            if (Check<ExtendsToken>())
            {
                Consume();
                var extendsToken = Expect<IdentifierToken>();
                extendsName = new IdentifiedNode(
                    extendsToken.Value,
                    extendsToken.Position
                );
            }

            Expect<OpenCurlyBracketToken>();

            List<AST> vardecs = [];
            // vardecs continue until constructor
            while (!Check<InitToken>())
            {
                vardecs.Add(ParseVariableDeclarationStatement());
            }

            AST constructor = ParseConstructor();

            List<AST> methodDefs = [];

            while (!Check<CloseCurlyBracketToken>())
            {
                methodDefs.Add(ParseMethodDefinition());
            }

            Expect<CloseCurlyBracketToken>();

            return new ClassDefinition(
                    new IdentifiedNode(
                        classNameToken.Value,
                        classNameToken.Position
                        ),
                    extendsName,
                    vardecs,
                    constructor,
                    methodDefs,
                    startToken.Position
                    );
        }


        // DEBUG/TESTING
        public static bool ASTsEqual(AST left, AST right, bool ignorePos=true) {
            var equal = (left, right) switch {
                (ProgramNode(var lClasses, var lStatements), 
                 ProgramNode(var rClasses, var rStatements)) => 
                    ASTListsEqual(lClasses, rClasses, ignorePos) && ASTListsEqual(lStatements, rStatements, ignorePos),

                (IntLiteral(var lVal, _), IntLiteral(var rVal, _)) => lVal == rVal,

                (StringLiteral(var lVal, _), StringLiteral(var rVal, _)) => lVal == rVal,

                (BooleanLiteral(var lVal, _), BooleanLiteral(var rVal, _)) => lVal == rVal,

                (IdentifiedNode(var lVal, _), IdentifiedNode(var rVal, _)) => lVal == rVal,

                (ThisExpression(_), ThisExpression(_)) => true,

                (BinaryExpression(var lLeft, var lOp, var lRight, _), BinaryExpression(var rLeft, var rOp, var rRight, _)) => 
                    lOp == rOp &&
					 ASTsEqual(lLeft, rLeft, ignorePos) &&
					 ASTsEqual(lRight, rRight, ignorePos),

                (MethodCallExpression(var lName, var lTarget, var lArguments, _), MethodCallExpression(var rName, var rTarget, var rArguments, _)) => 
                    lName == rName &&
					 ASTsEqual(lTarget, rTarget, ignorePos) &&
					 ASTListsEqual(lArguments, rArguments, ignorePos),

                (NewObjectExpression(var lClassName, var lArguments, _), NewObjectExpression(var rClassName, var rArguments, _)) => 
                    lClassName == rClassName &&
					 ASTListsEqual(lArguments, rArguments, ignorePos),

                (ExpressionStatement(var lExpression, _), ExpressionStatement(var rExpression, _)) => 
                    ASTsEqual(lExpression, rExpression, ignorePos),

                (VariableDeclaration(var lType, var lVar, _), VariableDeclaration(var rType, var rVar, _)) => 
                    ASTsEqual(lType, rType, ignorePos) &&
					 ASTsEqual(lVar, rVar, ignorePos),

                (AssignmentStatement(var lVar, var lVal, _), AssignmentStatement(var rVar, var rVal, _)) => 
                    ASTsEqual(lVar, rVar, ignorePos) &&
					 ASTsEqual(lVal, rVal, ignorePos),

                (WhileStatement(var lGuard, var lStatement, _), WhileStatement(var rGuard, var rStatement, _)) => 
                    ASTsEqual(lGuard, rGuard, ignorePos) &&
					 ASTsEqual(lStatement, rStatement, ignorePos),

                (BreakStatement(_), BreakStatement(_)) => true,

                (ReturnStatement(var lVal, _), ReturnStatement(var rVal, _)) => 
                    ASTsEqual(lVal, rVal, ignorePos),

                (IfStatement(var lGuard, var lIfBody, var lElseBody, _), IfStatement(var rGuard, var rIfBody, var rElseBody, _)) => 
                    ASTsEqual(lGuard, rGuard, ignorePos) &&
					 ASTsEqual(lIfBody, rIfBody, ignorePos) &&
					 ASTsEqual(lElseBody, rElseBody, ignorePos),

                (BlockStatement(var lStatements, _), BlockStatement(var rStatements, _)) => 
                    ASTListsEqual(lStatements, rStatements, ignorePos),

                (MethodDefinition(var lName, var lParameters, var lReturnType, var lBody, _), MethodDefinition(var rName, var rParameters, var rReturnType, var rBody, _)) => 
                    ASTsEqual(lName, rName, ignorePos) &&
					 ASTListsEqual(lParameters, rParameters, ignorePos) &&
					 ASTsEqual(lReturnType, rReturnType, ignorePos) &&
					 ASTsEqual(lBody, rBody, ignorePos),

                (Constructor(var lParameters, var lSuperArguments, var lStatements, _), Constructor(var rParameters, var rSuperArguments, var rStatements, _)) => 
                    ASTListsEqual(lParameters, rParameters, ignorePos) &&
					 ASTListsEqual(lSuperArguments, rSuperArguments, ignorePos) &&
					 ASTListsEqual(lStatements, rStatements, ignorePos),

                (ClassDefinition(var lName, var lExtendsName, var lVardecs, var lConstructor, var lMethodDefs, _), ClassDefinition(var rName, var rExtendsName, var rVardecs, var rConstructor, var rMethodDefs, _)) => 
                    ASTsEqual(lName, rName, ignorePos) &&
					 ASTsEqual(lExtendsName, rExtendsName, ignorePos) &&
					 ASTListsEqual(lVardecs, rVardecs, ignorePos) &&
					 ASTsEqual(lConstructor, rConstructor, ignorePos) &&
					 ASTListsEqual(lMethodDefs, rMethodDefs, ignorePos),

                (null, null) => true,
                _ => false
            };

            if(!equal) {
                Console.WriteLine($"{left} != {right}\n");
                return false;
            } else if(!ignorePos && left.Position != right.Position) {
                Console.WriteLine($"Positions differ: {left.Position} != {right.Position}\n");
                return false;
            }

            return true;
        }

        public static bool ASTListsEqual<T>(List<T> left, List<T> right, bool ignorePos=true) 
            where T: AST
        {
            if(left is null && right is null) return true;
            if(left is null || right is null) return false;
            
            return left.Count == right.Count &&
                left.Index().All(index => ASTsEqual(index.Item, right[index.Index], ignorePos));
        }
    }
}
