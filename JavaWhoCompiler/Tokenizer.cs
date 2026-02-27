using System.Text.RegularExpressions;

namespace JavaWhoCompiler
{
    public interface IToken
    {
        string Value { get; init; }
        int Line { get; init; }
        int Position { get; init; }
    }

    public class InvalidTokenException(string Message) : Exception(Message);

    public sealed record UnknownToken(string Value, int Line, int Position) : IToken;

    public sealed record IdentifierToken(string Value, int Line, int Position) : IToken;
    public sealed record WhiteSpaceToken(string Value, int Line, int Position) : IToken;
    public sealed record NewLineToken(string Value, int Line, int Position) : IToken;
    public sealed record StringToken(string Value, int Line, int Position) : IToken;
    public sealed record NumberToken(string Value, int Line, int Position) : IToken
    {
        public int Number => int.Parse(Value);
    }

    #region Operators
    public sealed record AddOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record SubtractOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record MultiplyOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record DivideOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record LessThanOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record EqualsOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record NotEqualsOperatorToken(string Value, int Line, int Position) : IToken;
    public sealed record AssignmentOperatorToken(string Value, int Line, int Position) : IToken;
    #endregion

    #region Reserved Keywords
    public sealed record ThisToken(string Value, int Line, int Position) : IToken;
    public sealed record TrueToken(string Value, int Line, int Position) : IToken;
    public sealed record FalseToken(string Value, int Line, int Position) : IToken;
    public sealed record NewToken(string Value, int Line, int Position) : IToken;
    public sealed record InitToken(string Value, int Line, int Position) : IToken;
    public sealed record ClassToken(string Value, int Line, int Position) : IToken;
    public sealed record SuperToken(string Value, int Line, int Position) : IToken;
    public sealed record WhileToken(string Value, int Line, int Position) : IToken;
    public sealed record BreakToken(string Value, int Line, int Position) : IToken;
    public sealed record IfToken(string Value, int Line, int Position) : IToken;
    public sealed record ElseToken(string Value, int Line, int Position) : IToken;
    public sealed record VoidTypeToken(string Value, int Line, int Position) : IToken;
    public sealed record MethodToken(string Value, int Line, int Position) : IToken;
    public sealed record ReturnToken(string Value, int Line, int Position) : IToken;
    public sealed record ExtendsToken(string Value, int Line, int Position) : IToken;
    #endregion

    #region Punctuation
    public sealed record OpenParenthesisToken(string Value, int Line, int Position) : IToken;
    public sealed record CloseParenthesisToken(string Value, int Line, int Position) : IToken;
    public sealed record OpenCurlyBracketToken(string Value, int Line, int Position) : IToken;
    public sealed record CloseCurlyBracketToken(string Value, int Line, int Position) : IToken;
    public sealed record SemiColonToken(string Value, int Line, int Position) : IToken;
    public sealed record CommaToken(string Value, int Line, int Position) : IToken;
    public sealed record DotToken(string Value, int Line, int Position) : IToken;
    #endregion

    public static class RegexPatterns
    {
        public static IReadOnlyDictionary<string, Func<string, int, int, IToken>> PatternToToken = new Dictionary<string, Func<string, int, int, IToken>>()
        {

            [@"\G[\p{Zs}\t]+"] = (value, line, pos) => new WhiteSpaceToken(value, line, pos),
            [@"\G\r?\n|\r"] = (value, line, pos) => new NewLineToken(value, line, pos),

            [@"\G\d+"] = (value, line, pos) => new NumberToken(value, line, pos),
            [@"\G""(?:\\.|[^""\\])*"""] = (value, line, pos) => new StringToken(value, line, pos),

            [@"\Gthis\b"] = (value, line, pos) => new ThisToken(value, line, pos),
            [@"\Gtrue\b"] = (value, line, pos) => new TrueToken(value, line, pos),
            [@"\Gfalse\b"] = (value, line, pos) => new FalseToken(value, line, pos),
            [@"\Gnew\b"] = (value, line, pos) => new NewToken(value, line, pos),
            [@"\Ginit\b"] = (value, line, pos) => new InitToken(value, line, pos),
            [@"\Gclass\b"] = (value, line, pos) => new ClassToken(value, line, pos),
            [@"\Gsuper\b"] = (value, line, pos) => new SuperToken(value, line, pos),
            [@"\Gwhile\b"] = (value, line, pos) => new WhileToken(value, line, pos),
            [@"\Gbreak\b"] = (value, line, pos) => new BreakToken(value, line, pos),
            [@"\Gif\b"] = (value, line, pos) => new IfToken(value, line, pos),
            [@"\Gelse\b"] = (value, line, pos) => new ElseToken(value, line, pos),
            [@"\GVoid\b"] = (value, line, pos) => new VoidTypeToken(value, line, pos),
            [@"\Gmethod\b"] = (value, line, pos) => new MethodToken(value, line, pos),
            [@"\Greturn\b"] = (value, line, pos) => new ReturnToken(value, line, pos),
            [@"\Gextends\b"] = (value, line, pos) => new ExtendsToken(value, line, pos),

            [@"\G[a-zA-Z_][a-zA-Z0-9_]*"] = (value, line, pos) => new IdentifierToken(value, line, pos),

            [@"\G\+"] = (value, line, pos) => new AddOperatorToken(value, line, pos),
            [@"\G\-"] = (value, line, pos) => new SubtractOperatorToken(value, line, pos),
            [@"\G\*"] = (value, line, pos) => new MultiplyOperatorToken(value, line, pos),
            [@"\G\/"] = (value, line, pos) => new DivideOperatorToken(value, line, pos),
            [@"\G\=="] = (value, line, pos) => new EqualsOperatorToken(value, line, pos),
            [@"\G\!="] = (value, line, pos) => new NotEqualsOperatorToken(value, line, pos),
            [@"\G\="] = (value, line, pos) => new AssignmentOperatorToken(value, line, pos),
            [@"\G\<"] = (value, line, pos) => new LessThanOperatorToken(value, line, pos),

            [@"\G\("] = (value, line, pos) => new OpenParenthesisToken(value, line, pos),
            [@"\G\)"] = (value, line, pos) => new CloseParenthesisToken(value, line, pos),
            [@"\G\{"] = (value, line, pos) => new OpenCurlyBracketToken(value, line, pos),
            [@"\G\}"] = (value, line, pos) => new CloseCurlyBracketToken(value, line, pos),
            [@"\G;"] = (value, line, pos) => new SemiColonToken(value, line, pos),
            [@"\G,"] = (value, line, pos) => new CommaToken(value, line, pos),
            [@"\G\."] = (value, line, pos) => new DotToken(value, line, pos)
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
                    value: new UnknownToken("", 0, 0)
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
                    throw new InvalidTokenException($"Invalid token '{code[i]}' starting at position: {i}");
                }

                i += tokenMatch.Key.Length - 1;
                position += tokenMatch.Key.Length - 1;

                if (tokenMatch.Value is NewLineToken) {
                    line += 1;
                    position = 1;
                }

                // ignore whitespace
                if (tokenMatch.Value is not WhiteSpaceToken) {
                    tokens.Add(tokenMatch.Value);
                }

            }

            return tokens;
        }
    }
}
