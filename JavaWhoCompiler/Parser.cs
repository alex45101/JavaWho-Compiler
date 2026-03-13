using System.Diagnostics;

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
        public static bool ASTsEqual(AST left, AST right, bool ignorePos=true)
        {
            bool equal = (left, right) switch {
                (ProgramNode(List<AST> lClasses, List<AST> lStatements), 
                 ProgramNode(List<AST> rClasses, List<AST> rStatements)) => 
                    ASTListsEqual(lClasses, rClasses, ignorePos) && ASTListsEqual(lStatements, rStatements, ignorePos),

                (IntLiteral(int lVal, _), 
                 IntLiteral(int rVal, _)) => lVal == rVal,

                (StringLiteral(string lVal, _), 
                 StringLiteral(string rVal, _)) => lVal == rVal,

                (BooleanLiteral(bool lVal, _), 
                 BooleanLiteral(bool rVal, _)) => lVal == rVal,

                (IdentifiedNode(string lVal, _), 
                 IdentifiedNode(string rVal, _)) => lVal == rVal,

                (ThisExpression, 
                 ThisExpression) => true,

                (BinaryExpression(AST lLeft, OperatorType lOp, AST lRight, _), 
                 BinaryExpression(AST rLeft, OperatorType rOp, AST rRight, _)) => 
                    lOp == rOp &&
					 ASTsEqual(lLeft, rLeft, ignorePos) &&
					 ASTsEqual(lRight, rRight, ignorePos),

                // target is null in println calls
                (MethodCallExpression(string lName, var lTarget, List<AST> lArguments, _), 
                 MethodCallExpression(string rName, var rTarget, List<AST> rArguments, _)) => 
                    lName == rName &&
					 ASTsEqual(lTarget, rTarget, ignorePos) &&
					 ASTListsEqual(lArguments, rArguments, ignorePos),

                (NewObjectExpression(AST lClassName, List<AST> lArguments, _), 
                 NewObjectExpression(AST rClassName, List<AST> rArguments, _)) => 
                    lClassName == rClassName &&
					 ASTListsEqual(lArguments, rArguments, ignorePos),

                (ExpressionStatement(AST lExpression, _), 
                 ExpressionStatement(AST rExpression, _)) => 
                    ASTsEqual(lExpression, rExpression, ignorePos),

                (VariableDeclaration(AST lType, AST lVar, _), 
                 VariableDeclaration(AST rType, AST rVar, _)) => 
                    ASTsEqual(lType, rType, ignorePos) &&
					 ASTsEqual(lVar, rVar, ignorePos),

                (AssignmentStatement(AST lVar, AST lVal, _), 
                 AssignmentStatement(AST rVar, AST rVal, _)) => 
                    ASTsEqual(lVar, rVar, ignorePos) &&
					 ASTsEqual(lVal, rVal, ignorePos),

                (WhileStatement(AST lGuard, AST lStatement, _), 
                 WhileStatement(AST rGuard, AST rStatement, _)) => 
                    ASTsEqual(lGuard, rGuard, ignorePos) &&
					 ASTsEqual(lStatement, rStatement, ignorePos),

                (BreakStatement, 
                 BreakStatement) => true,

                // optional value in return
                (ReturnStatement(var lVal, _), 
                 ReturnStatement(var rVal, _)) => 
                    ASTsEqual(lVal, rVal, ignorePos),

                // optional else
                (IfStatement(AST lGuard, AST lIfBody, var lElseBody, _), 
                 IfStatement(AST rGuard, AST rIfBody, var rElseBody, _)) => 
                    ASTsEqual(lGuard, rGuard, ignorePos) &&
					 ASTsEqual(lIfBody, rIfBody, ignorePos) &&
					 ASTsEqual(lElseBody, rElseBody, ignorePos),

                (BlockStatement(List<AST> lStatements, _), 
                 BlockStatement(List<AST> rStatements, _)) => 
                    ASTListsEqual(lStatements, rStatements, ignorePos),

                // returning Void is interpreted as null
                (MethodDefinition(AST lName, List<AST> lParameters, var lReturnType, AST lBody, _),
                 MethodDefinition(AST rName, List<AST> rParameters, var rReturnType, AST rBody, _)) => 
                    ASTsEqual(lName, rName, ignorePos) &&
					 ASTListsEqual(lParameters, rParameters, ignorePos) &&
					 ASTsEqual(lReturnType, rReturnType, ignorePos) &&
					 ASTsEqual(lBody, rBody, ignorePos),

                // optional super call
                (Constructor(List<AST> lParameters, var lSuperArguments, List<AST> lStatements, _), 
                 Constructor(List<AST> rParameters, var rSuperArguments, List<AST> rStatements, _)) => 
                    ASTListsEqual(lParameters, rParameters, ignorePos) &&
					 ASTListsEqual(lSuperArguments, rSuperArguments, ignorePos) &&
					 ASTListsEqual(lStatements, rStatements, ignorePos),

                // optional extend
                (ClassDefinition(AST lName, var lExtendsName, List<AST> lVardecs, AST lConstructor, List<AST> lMethodDefs, _), 
                 ClassDefinition(AST rName, var rExtendsName, List<AST> rVardecs, AST rConstructor, List<AST> rMethodDefs, _)) => 
                    ASTsEqual(lName, rName, ignorePos) &&
					 ASTsEqual(lExtendsName, rExtendsName, ignorePos) &&
					 ASTListsEqual(lVardecs, rVardecs, ignorePos) &&
					 ASTsEqual(lConstructor, rConstructor, ignorePos) &&
					 ASTListsEqual(lMethodDefs, rMethodDefs, ignorePos),

                (null, null) => true,
                _ => false
            };

            if(!equal) {
                Debug.WriteLine($"{left} != {right}\n");
                return false;
            }

            // left and right could be null here
            if(!ignorePos && left is not null && left.Position != right.Position) {
                Debug.WriteLine($"Positions differ: {left.Position} != {right.Position}\n");
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
