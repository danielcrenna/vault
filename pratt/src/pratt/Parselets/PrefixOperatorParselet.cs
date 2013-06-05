using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Generic prefix parselet for an unary arithmetic operator; parses prefix unary "-", "+", "~", and "!" expressions
    /// </summary>
    public class PrefixOperatorParselet : IPrefixParselet
    {
        private readonly TokenType _operator;
        private readonly Precedence _precedence;

        public PrefixOperatorParselet(TokenType @operator, Precedence precedence)
        {
            _operator = @operator;
            _precedence = precedence;
        }

        public IExpression Parse(Parser parser, Token token)
        {
            // To handle right-associative operators like "^", we allow a slightly
            // lower precedence when parsing the right-hand side. This will let a
            // parselet with the same precedence appear on the right, which will then
            // take *this* parselet's result as its left-hand argument.
            var right = parser.ParseExpression(_precedence);

            return new PrefixExpression(_operator, right);
        }
    }
}