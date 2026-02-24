namespace JavaWhoCompiler
{
    public interface IToken
    {
        string Value { get; init; }
    }


    //
    public sealed record IdentifierToken(string Value) : IToken;
    public sealed record WhiteSpaceToken(string Value) : IToken;
    public sealed record StringToken(string Value) : IToken;
    public sealed record NumberToken(string Value) : IToken
    {
        int Number => int.Parse(Value);
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
    #endregion

    #region Punctuation
    public sealed record OpenParenthesisToken(string Value) : IToken;
    public sealed record CloseParenthesisToken(string Value) : IToken;
    public sealed record OpenCurlyBracketToken(string Value) : IToken;
    public sealed record CloseCurlyBracketToken(string Value) : IToken;
    public sealed record SemiColonToken(string Value) : IToken;
    public sealed record CommaToken(string Value) : IToken;
    public sealed record PeriodToken(string Value) : IToken;
    #endregion

    public static class Tokenizer
    {
    }
}
