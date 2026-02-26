using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

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
    public sealed record PrimaryExpression(string Value) : AST;
    public sealed record BinaryExpression(AST Left, OperatorType OperatorType, AST Right) : AST;


    //statements
    public sealed record ExpStmt(AST Exp) : AST;
    public sealed record VardecStmt(IdentifiedNode Type, IdentifiedNode Var) : AST;
    public sealed record AssignStmt(IdentifiedNode Var, AST Val) : AST;
    public sealed record WhileStmt(AST Guard, AST Stmt) : AST;
    public sealed record BreakStmt() : AST;
    public sealed record ReturnStmt(AST Val) : AST;
    public sealed record IfStmt(AST Guard, AST IfBody, AST ElseBody) : AST;
    public sealed record BlockStmt(List<AST> Stmts) : AST;


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
                WhileToken => ParseWhileStmt(),
                BreakToken => ParseBreakStmt(),
                ReturnToken => ParseReturnStmt(),
                IfToken => ParseIfStmt(),
                OpenCurlyBracketToken => ParseBlockStmt(),
                IdentifierToken => PeekNext() switch {
                    IdentifierToken => ParseVardecStmt(),
                    AssignmentOperatorToken => ParseAssignStmt(),

                    // no token ahead of identifier
                    null => throw new ParserException("Expected ';', got EOF"),

                    // try expression statement
                    _ => ParseExpressionStmt(),
                },

                // try expression statement
                _ => ParseExpressionStmt(),
            };

            return stmt;
        }

        private AST ParseWhileStmt() {
            Console.WriteLine("WHILE");
            Expect<WhileToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST body = ParseStatement();

            return new WhileStmt(guard, body);
        }

        private AST ParseBreakStmt() {
            Console.WriteLine("BREAK");
            Expect<BreakToken>();
            Expect<SemiColonToken>();

            return new BreakStmt();
        }

        private AST ParseReturnStmt() {
            Console.WriteLine("RETURN");
            Expect<ReturnToken>();

            AST val = null;
            try {
                val = ParseExpression();
            } catch {}

            Expect<SemiColonToken>();

            return new ReturnStmt(val);
        }

        private AST ParseIfStmt() {
            Console.WriteLine("IF");
            Expect<IfToken>();
            Expect<OpenParenthesisToken>();

            AST guard = ParseExpression();

            Expect<CloseParenthesisToken>();

            AST ifBody = ParseStatement();

            AST elseBody = null;

            // we don't care if an exception is thrown on the 'else' expect
            try {
                Expect<ElseToken>();
            } catch { return new IfStmt(guard, ifBody, elseBody); }

            elseBody = ParseStatement();

            return new IfStmt(guard, ifBody, elseBody);
        }

        private AST ParseBlockStmt() {
            Console.WriteLine("BLOCK");
            Expect<OpenCurlyBracketToken>();

            List<AST> stmts = [];
            while(CurrentToken is not CloseCurlyBracketToken) {
                stmts.Add(ParseStatement());
            }

            // consume closing bracket
            Consume();

            return new BlockStmt(stmts);
        }

        private AST ParseVardecStmt() {
            Console.WriteLine("VARDEC");
            
            string typeIdent = Expect<IdentifierToken>().Value;

            
            string varIdent = Expect<IdentifierToken>().Value;

            Expect<SemiColonToken>();
            
            return new VardecStmt(
                    new IdentifiedNode(typeIdent), 
                    new IdentifiedNode(varIdent)
                    );
        }

        private AST ParseAssignStmt() {
            Console.WriteLine("ASSIGN");
            
            string varIdent = Expect<IdentifierToken>().Value;

            Expect<AssignmentOperatorToken>();
            
            AST value = ParseExpression();

            Expect<SemiColonToken>();

            return new AssignStmt(
                    new IdentifiedNode(varIdent), 
                    value
                    );
        }

        private AST ParseExpressionStmt() {
            Console.WriteLine("EXP");
            AST exp = ParseExpression();

            Expect<SemiColonToken>();

            return new ExpStmt(exp);
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

            if (Check<AddOperatorToken>() || Check<SubtractOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is AddOperatorToken ? OperatorType.Add : OperatorType.Subtract;

                Consume();

                AST right = ParseMultiplyExpression();

                return new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseMultiplyExpression()
        {
            AST left = ParseCallExpression();

            if (Check<MultiplyOperatorToken>() || Check<DivideOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is MultiplyOperatorToken ? OperatorType.Multiply : OperatorType.Divide;

                Consume();

                AST right = ParseCallExpression();

                return new BinaryExpression(left, operatorType, right);
            }

            return left;
        }

        private AST ParseCallExpression()
        {
            AST left = ParsePrimaryExpression();

            if (Check<DotToken>())
            {
                //need to implement dot stuff
                throw new NotImplementedException();
            }

            return left;
        }

        private AST ParsePrimaryExpression()
        {
            AST primaryNode = CurrentToken switch
            {
                IdentifierToken => new IdentifiedNode(Consume().Value),
                NumberToken => new IntLiteral((Consume() as NumberToken).Number),
                TrueToken or FalseToken => new BooleanLiteral(Consume().Value == "true"),
                _ => throw new ParserException($"Unexpected token {CurrentToken.GetType().Name}")
            };

            return primaryNode;
        }
    }
}
