using System.Text.RegularExpressions;

namespace JavaWhoCompiler
{
    public sealed record Position(int Line, int Column);

    public interface IToken
    {
        string Value { get; init; }
        Position Position { get; init; }
    }

    public class InvalidTokenException(string Message) : Exception(Message);

    public sealed record UnknownToken(string Value, Position Position) : IToken;

    public sealed record IdentifierToken(string Value, Position Position) : IToken;
    public sealed record WhiteSpaceToken(string Value, Position Position) : IToken;
    public sealed record NewLineToken(string Value, Position Position) : IToken;
    public sealed record StringToken(string Value, Position Position) : IToken;
    public sealed record NumberToken(string Value, Position Position) : IToken
    {
        public int Number => int.Parse(Value);
    }

    #region Operators
    public sealed record AddOperatorToken(string Value, Position Position) : IToken;
    public sealed record SubtractOperatorToken(string Value, Position Position) : IToken;
    public sealed record MultiplyOperatorToken(string Value, Position Position) : IToken;
    public sealed record DivideOperatorToken(string Value, Position Position) : IToken;
    public sealed record LessThanOperatorToken(string Value, Position Position) : IToken;
    public sealed record EqualsOperatorToken(string Value, Position Position) : IToken;
    public sealed record NotEqualsOperatorToken(string Value, Position Position) : IToken;
    public sealed record AssignmentOperatorToken(string Value, Position Position) : IToken;
    #endregion

    #region Reserved Keywords
    public sealed record ThisToken(string Value, Position Position) : IToken;
    public sealed record TrueToken(string Value, Position Position) : IToken;
    public sealed record FalseToken(string Value, Position Position) : IToken;
    public sealed record NewToken(string Value, Position Position) : IToken;
    public sealed record InitToken(string Value, Position Position) : IToken;
    public sealed record ClassToken(string Value, Position Position) : IToken;
    public sealed record SuperToken(string Value, Position Position) : IToken;
    public sealed record WhileToken(string Value, Position Position) : IToken;
    public sealed record BreakToken(string Value, Position Position) : IToken;
    public sealed record IfToken(string Value, Position Position) : IToken;
    public sealed record ElseToken(string Value, Position Position) : IToken;
    public sealed record VoidTypeToken(string Value, Position Position) : IToken;
    public sealed record MethodToken(string Value, Position Position) : IToken;
    public sealed record ReturnToken(string Value, Position Position) : IToken;
    public sealed record ExtendsToken(string Value, Position Position) : IToken;
    public sealed record PrintLnToken(string Value, Position Position) : IToken;
    #endregion

    #region Punctuation
    public sealed record OpenParenthesisToken(string Value, Position Position) : IToken;
    public sealed record CloseParenthesisToken(string Value, Position Position) : IToken;
    public sealed record OpenCurlyBracketToken(string Value, Position Position) : IToken;
    public sealed record CloseCurlyBracketToken(string Value, Position Position) : IToken;
    public sealed record SemiColonToken(string Value, Position Position) : IToken;
    public sealed record CommaToken(string Value, Position Position) : IToken;
    public sealed record DotToken(string Value, Position Position) : IToken;
    #endregion

    public static class RegexPatterns
    {
        public static IReadOnlyDictionary<string, Func<string, int, int, IToken>> PatternToToken = new Dictionary<string, Func<string, int, int, IToken>>()
        {

            [@"\G[\p{Zs}\t]+"] = (value, line, col) => new WhiteSpaceToken(value, new Position(line, col)),
            [@"\G(\r?\n|\r)"] = (value, line, col) => new NewLineToken(value, new Position(line, col)),

            [@"\G\d+"] = (value, line, col) => new NumberToken(value, new Position(line, col)),
            [@"\G""(?:\\.|[^""\\])*"""] = (value, line, col) => new StringToken(value, new Position(line, col)),

            [@"\Gthis\b"] = (value, line, col) => new ThisToken(value, new Position(line, col)),
            [@"\Gtrue\b"] = (value, line, col) => new TrueToken(value, new Position(line, col)),
            [@"\Gfalse\b"] = (value, line, col) => new FalseToken(value, new Position(line, col)),
            [@"\Gnew\b"] = (value, line, col) => new NewToken(value, new Position(line, col)),
            [@"\Ginit\b"] = (value, line, col) => new InitToken(value, new Position(line, col)),
            [@"\Gclass\b"] = (value, line, col) => new ClassToken(value, new Position(line, col)),
            [@"\Gsuper\b"] = (value, line, col) => new SuperToken(value, new Position(line, col)),
            [@"\Gwhile\b"] = (value, line, col) => new WhileToken(value, new Position(line, col)),
            [@"\Gbreak\b"] = (value, line, col) => new BreakToken(value, new Position(line, col)),
            [@"\Gif\b"] = (value, line, col) => new IfToken(value, new Position(line, col)),
            [@"\Gelse\b"] = (value, line, col) => new ElseToken(value, new Position(line, col)),
            [@"\GVoid\b"] = (value, line, col) => new VoidTypeToken(value, new Position(line, col)),
            [@"\Gmethod\b"] = (value, line, col) => new MethodToken(value, new Position(line, col)),
            [@"\Greturn\b"] = (value, line, col) => new ReturnToken(value, new Position(line, col)),
            [@"\Gextends\b"] = (value, line, col) => new ExtendsToken(value, new Position(line, col)),
            [@"\Gprintln\b"] = (value, line, col) => new PrintLnToken(value, new Position(line, col)),

            [@"\G[a-zA-Z_][a-zA-Z0-9_]*"] = (value, line, col) => new IdentifierToken(value, new Position(line, col)),

            [@"\G\+"] = (value, line, col) => new AddOperatorToken(value, new Position(line, col)),
            [@"\G\-"] = (value, line, col) => new SubtractOperatorToken(value, new Position(line, col)),
            [@"\G\*"] = (value, line, col) => new MultiplyOperatorToken(value, new Position(line, col)),
            [@"\G\/"] = (value, line, col) => new DivideOperatorToken(value, new Position(line, col)),
            [@"\G\=="] = (value, line, col) => new EqualsOperatorToken(value, new Position(line, col)),
            [@"\G\!="] = (value, line, col) => new NotEqualsOperatorToken(value, new Position(line, col)),
            [@"\G\="] = (value, line, col) => new AssignmentOperatorToken(value, new Position(line, col)),
            [@"\G\<"] = (value, line, col) => new LessThanOperatorToken(value, new Position(line, col)),

            [@"\G\("] = (value, line, col) => new OpenParenthesisToken(value, new Position(line, col)),
            [@"\G\)"] = (value, line, col) => new CloseParenthesisToken(value, new Position(line, col)),
            [@"\G\{"] = (value, line, col) => new OpenCurlyBracketToken(value, new Position(line, col)),
            [@"\G\}"] = (value, line, col) => new CloseCurlyBracketToken(value, new Position(line, col)),
            [@"\G;"] = (value, line, col) => new SemiColonToken(value, new Position(line, col)),
            [@"\G,"] = (value, line, col) => new CommaToken(value, new Position(line, col)),
            [@"\G\."] = (value, line, col) => new DotToken(value, new Position(line, col))
        };
    }

    public static class Tokenizer
    {
        public static IEnumerable<IToken> Tokenize(string code)
        {
            List<IToken> tokens = new List<IToken>();
            int line = 1, position = 1;

            for (int i = 0; i < code.Length; i++)
            {
                KeyValuePair<Match, IToken> tokenMatch = new KeyValuePair<Match, IToken>(
                    key: Match.Empty,
                    value: new UnknownToken("", null)
                );

                foreach (KeyValuePair<string, Func<string, int, int, IToken>> pattern in RegexPatterns.PatternToToken)
                {
                    Regex regex = new Regex(pattern.Key);

                    Match currMatch = regex.Match(code, i);

                    if (currMatch.Length > 0)
                    {
                        tokenMatch = new KeyValuePair<Match, IToken>(
                            key: currMatch,
                            value: pattern.Value(currMatch.Value, line, position)
                        );
                        break;
                    }
                }


                if (tokenMatch.Value is UnknownToken)
                {
                    throw new InvalidTokenException($"Invalid token '{code[i]}' on line {line} position {position}");
                }


                i += tokenMatch.Key.Length - 1;
                position += tokenMatch.Key.Length;

                if (tokenMatch.Value is NewLineToken) {
                    line += 1;
                    position = 1;
                }

                // ignore whitespace
                if (tokenMatch.Value is not WhiteSpaceToken && tokenMatch.Value is not NewLineToken) {
                    tokens.Add(tokenMatch.Value);
                }

            }

            return tokens;
        }
    }
}
