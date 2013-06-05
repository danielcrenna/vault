using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Generic infix parselet for an unary arithmetic operator; parses postfix unary "?" expressions
    /// </summary>
    public class PostfixOperatorParselet : IInfixParselet
    {
        private readonly TokenType _operator;
        private readonly Precedence _precedence;

        public PostfixOperatorParselet(TokenType @operator, Precedence precedence)
        {
            _operator = @operator;
            _precedence = precedence;
        }

        public IExpression Parse(Parser parser, IExpression left, Token token)
        {
            return new PostfixExpression(left, _operator);
        }

        public Precedence Precedence
        {
            get { return _precedence; }
        }
    }
}