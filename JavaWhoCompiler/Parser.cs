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

    public abstract record ParseNode;
    public sealed record ParseStatement(List<ParseNode> Classes, List<ParseNode> Statements) : ParseNode;

    //expressions
    public sealed record PrimaryParseNode(string Value) : ParseNode;
    public sealed record BinaryParseNode(ParseNode Left, OperatorType OperatorType, ParseNode Right) : ParseNode;

    #region WIP AST
    public abstract record AST;

    public sealed record BinaryExpression(AST Left, OperatorType Operator, AST Right) : AST;
    public sealed record VariableExpression(string Name) : AST;
    public sealed record IntLiteralNode(int Value) : AST;
    #endregion  

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


        public static AST Parse(IEnumerable<IToken> tokens)
        {
            AST root = null;

            Parser parser = new Parser(tokens);
            List<ParseNode> statements = new List<ParseNode>();

            while (!parser.IsEnd)
            {
                statements.Add(parser.ParseStatement());
            }
            
            return root;
        }

        private Parser(IEnumerable<IToken> tokens)
        {
            this.tokens = tokens.ToArray();
            currPos = 0;
        }

        private void Expect<T>() where T : IToken
        {
            if (!Check<T>())
            {
                throw new ParserException($"Expected {typeof(T)} but current token is {CurrentToken.GetType()}");
            }

            Consume();
        }

        private ParseNode ParseStatement()
        {
            ParseNode exp = ParseExpression();

            Expect<SemiColonToken>();
            return exp;
        }

        private ParseNode ParseExpression() => ParseEqualityExpression();

        private ParseNode ParseEqualityExpression()
        {
            ParseNode left = ParseCompareExpression();

            if (Check<EqualsOperatorToken>() || Check<NotEqualsOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is EqualsOperatorToken ? OperatorType.Equal : OperatorType.NotEqual;
                
                Consume();

                ParseNode right = ParseCompareExpression();

                return new BinaryParseNode(left, operatorType, right);
            }

            return left;
        }

        private ParseNode ParseCompareExpression()
        {
            ParseNode left = ParseAddExpression();

            if (Check<LessThanOperatorToken>())
            {
                OperatorType operatorType = OperatorType.LessThan;

                Consume();

                ParseNode right = ParseAddExpression();

                return new BinaryParseNode(left, operatorType, right);
            }

            return left;
        }

        private ParseNode ParseAddExpression()
        {
            ParseNode left = ParseMultiplyExpression();

            if (Check<AddOperatorToken>() || Check<SubtractOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is AddOperatorToken ? OperatorType.Add : OperatorType.Subtract;

                Consume();

                ParseNode right = ParseMultiplyExpression();

                return new BinaryParseNode(left, operatorType, right);
            }

            return left;
        }

        private ParseNode ParseMultiplyExpression()
        {
            ParseNode left = ParseCallExpression();

            if (Check<MultiplyOperatorToken>() || Check<DivideOperatorToken>())
            {
                OperatorType operatorType = CurrentToken is MultiplyOperatorToken ? OperatorType.Multiply : OperatorType.Divide;

                Consume();

                ParseNode right = ParseCallExpression();

                return new BinaryParseNode(left, operatorType, right);
            }

            return left;
        }

        private ParseNode ParseCallExpression()
        {
            ParseNode left = ParsePrimaryExpression();

            if (Check<DotToken>())
            {
                throw new NotImplementedException();
            }

            return left;
        }

        private ParseNode ParsePrimaryExpression()
        {
            PrimaryParseNode primaryNode = CurrentToken switch
            {
                IdentifierToken or 
                NumberToken => new PrimaryParseNode(Consume().Value),
                _ => throw new ParserException($"Unexpected token {CurrentToken.GetType().Name}")
            };

            return primaryNode;
        }
    }
}
