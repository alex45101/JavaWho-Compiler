using System;
using System.Collections.Generic;
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

    public sealed record BinaryExpression(AST Left, OperatorType Operator, AST Right) : AST;
    public sealed record VariableExpression(string Name) : AST;
    public sealed record IntLiteralNode(int Value) : AST;

    public class ParserException(string message) : Exception(message);

    public class Parser
    {
        private static IReadOnlyDictionary<Type, List<List<Type>>> TokenPatterns = new Dictionary<Type, List<List<Type>>>()
        {
            [typeof(IdentifierToken)] = [[typeof(LessThanOperatorToken), typeof(NumberToken)]]
        };

        private IToken[] tokens = null;
        private int currPos = 0;

        private IToken CurrentToken => currPos < tokens.Length ? tokens[currPos] : throw new IndexOutOfRangeException();
        private IToken Consume() => currPos < tokens.Length ? tokens[currPos++] : throw new IndexOutOfRangeException();
        private IToken GetTokenAt(int pos) => pos < tokens.Length ? tokens[pos] : throw new IndexOutOfRangeException();

        public static AST Parse(IEnumerable<IToken> tokens)
        {
            AST root = null;

            Parser parser = new Parser()
            {
                tokens = tokens.ToArray(),
                currPos = 0
            };

            while (parser.CurrentToken is not null)
            {
                IToken currentToken = parser.Consume();

                Type currentTokenType = currentToken.GetType();

                if (TokenPatterns.TryGetValue(currentTokenType, out List<List<Type>> patterns))
                {
                    foreach (List<Type> pattern in patterns)
                    {
                        for (int i = 0; i < pattern.Count; i++)
                        {
                            IToken nextPatternToken = parser.GetTokenAt(parser.currPos + i + 1);
                        }
                    }

                    continue;
                }

                throw new ParserException($"No pattern match found for token: {currentToken}");
            }

            return root;
        }
    }
}
