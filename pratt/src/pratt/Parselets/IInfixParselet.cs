using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// One of the two parselet interfaces used by the Pratt parser. An
    /// InfixParselet is associated with a token that appears in the middle of the
    /// expression it parses. Its Parse() method will be called after the left-hand
    /// side has been parsed, and it in turn is responsible for parsing everything
    /// that comes after the token. This is also used for postfix expressions, in
    /// which case it simply doesn't consume any more tokens in its Parse() call.
    /// </summary>
    public interface IInfixParselet
    {
        IExpression Parse(Parser parser, IExpression left, Token token);
        Precedence Precedence { get; }
    }
}
