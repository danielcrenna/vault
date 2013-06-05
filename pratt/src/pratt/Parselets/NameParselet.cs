using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Simple parselet for a named variable like "abc"
    /// </summary>
    public class NameParselet : IPrefixParselet
    {
        public IExpression Parse(Parser parser, Token token)
        {
            return new NameExpression(token.Value);
        }
    }
}