using System.Text.RegularExpressions;

namespace JavaWhoCompiler
{
    public interface IToken
    {
        string Value { get; init; }
    }

    public class InvalidTokenException(string Message) : Exception(Message);

    public sealed record UnknownToken(string Value) : IToken;

    public sealed record IdentifierToken(string Value) : IToken;
    public sealed record WhiteSpaceToken(string Value) : IToken;
    public sealed record StringToken(string Value) : IToken;
    public sealed record NumberToken(string Value) : IToken
    {
        public int Number => int.Parse(Value);
    }

    #region Operators
    public sealed record AddOperatorToken(string Value) : IToken;
    public sealed record SubtractOperatorToken(string Value) : IToken;
    public sealed record MultiplyOperatorToken(string Value) : IToken;
    public sealed record DivideOperatorToken(string Value) : IToken;
    public sealed record LessThanOperatorToken(string Value) : IToken;
    public sealed record EqualsOperatorToken(string Value) : IToken;
    public sealed record NotEqualsOperatorToken(string Value) : IToken;
    public sealed record AssignmentOperatorToken(string Value) : IToken;
    #endregion

    #region Reserved Keywords
    public sealed record ThisToken(string Value) : IToken;
    public sealed record TrueToken(string Value) : IToken;
    public sealed record FalseToken(string Value) : IToken;
    public sealed record NewToken(string Value) : IToken;
    public sealed record InitToken(string Value) : IToken;
    public sealed record ClassToken(string Value) : IToken;
    public sealed record SuperToken(string Value) : IToken;
    public sealed record WhileToken(string Value) : IToken;
    public sealed record BreakToken(string Value) : IToken;
    public sealed record IfToken(string Value) : IToken;
    public sealed record ElseToken(string Value) : IToken;
    public sealed record VoidTypeToken(string Value) : IToken;
    public sealed record MethodToken(string Value) : IToken;
    public sealed record ReturnToken(string Value) : IToken;
    public sealed record ExtendsToken(string Value) : IToken;
    public sealed record PrintLnToken(string Value) : IToken;
    #endregion

    #region Punctuation
    public sealed record OpenParenthesisToken(string Value) : IToken;
    public sealed record CloseParenthesisToken(string Value) : IToken;
    public sealed record OpenCurlyBracketToken(string Value) : IToken;
    public sealed record CloseCurlyBracketToken(string Value) : IToken;
    public sealed record SemiColonToken(string Value) : IToken;
    public sealed record CommaToken(string Value) : IToken;
    public sealed record DotToken(string Value) : IToken;
    #endregion

    public static class RegexPatterns
    {
        public static IReadOnlyDictionary<string, Func<string, IToken>> PatternToToken = new Dictionary<string, Func<string, IToken>>()
        {

            [@"\G\s+"] = value => new WhiteSpaceToken(value),

            [@"\G\d+"] = value => new NumberToken(value),
            [@"\G""(?:\\.|[^""\\])*"""] = value => new StringToken(value),

            [@"\Gthis\b"] = value => new ThisToken(value),
            [@"\Gtrue\b"] = value => new TrueToken(value),
            [@"\Gfalse\b"] = value => new FalseToken(value),
            [@"\Gnew\b"] = value => new NewToken(value),
            [@"\Ginit\b"] = value => new InitToken(value),
            [@"\Gclass\b"] = value => new ClassToken(value),
            [@"\Gsuper\b"] = value => new SuperToken(value),
            [@"\Gwhile\b"] = value => new WhileToken(value),
            [@"\Gbreak\b"] = value => new BreakToken(value),
            [@"\Gif\b"] = value => new IfToken(value),
            [@"\Gelse\b"] = value => new ElseToken(value),
            [@"\GVoid\b"] = value => new VoidTypeToken(value),
            [@"\Gmethod\b"] = value => new MethodToken(value),
            [@"\Greturn\b"] = value => new ReturnToken(value),
            [@"\Gextends\b"] = value => new ExtendsToken(value),
            [@"\Gprintln\b"] = value => new PrintLnToken(value),

            [@"\G[a-zA-Z_][a-zA-Z0-9_]*"] = value => new IdentifierToken(value),

            [@"\G\+"] = value => new AddOperatorToken(value),
            [@"\G\-"] = value => new SubtractOperatorToken(value),
            [@"\G\*"] = value => new MultiplyOperatorToken(value),
            [@"\G\/"] = value => new DivideOperatorToken(value),
            [@"\G\=="] = value => new EqualsOperatorToken(value),
            [@"\G\!="] = value => new NotEqualsOperatorToken(value),
            [@"\G\="] = value => new AssignmentOperatorToken(value),
            [@"\G\<"] = value => new LessThanOperatorToken(value),

            [@"\G\("] = value => new OpenParenthesisToken(value),
            [@"\G\)"] = value => new CloseParenthesisToken(value),
            [@"\G\{"] = value => new OpenCurlyBracketToken(value),
            [@"\G\}"] = value => new CloseCurlyBracketToken(value),
            [@"\G;"] = value => new SemiColonToken(value),
            [@"\G,"] = value => new CommaToken(value),
            [@"\G\."] = value => new DotToken(value)
        };
    }

    public static class Tokenizer
    {
        public static IEnumerable<IToken> Tokenize(string code)
        {
            List<IToken> tokens = new List<IToken>();

            for (int i = 0; i < code.Length; i++)
            {
                KeyValuePair<Match, IToken> tokenMatch = new KeyValuePair<Match, IToken>(
                    key: Match.Empty,
                    value: new UnknownToken("")
                );

                foreach (KeyValuePair<string, Func<string, IToken>> pattern in RegexPatterns.PatternToToken)
                {
                    Regex regex = new Regex(pattern.Key);

                    Match currMatch = regex.Match(code, i);

                    if (currMatch.Length > 0)
                    {
                        tokenMatch = new KeyValuePair<Match, IToken>(
                            key: currMatch,
                            value: pattern.Value(currMatch.Value)
                        );
                        break;
                    }
                }


                if (tokenMatch.Value is UnknownToken)
                {
                    throw new InvalidTokenException($"Invalid token '{code[i]}' starting at position: {i}");
                }

                i += tokenMatch.Key.Length - 1;

                // ignore whitespace
                if (tokenMatch.Value is not WhiteSpaceToken) {
                    tokens.Add(tokenMatch.Value);
                }

            }

            return tokens;
        }
    }
}
