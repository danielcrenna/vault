using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Parses parentheses used to group an expression, like "a * (b + c)".
    /// </summary>
    public class GroupParselet : IPrefixParselet
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var expression = parser.ParseExpression();
            parser.Consume(TokenType.RightParen);
            return expression;
        }
    }
}