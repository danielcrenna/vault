using Pratt;
using pratt.Expressions;

namespace pratt.Parselets
{
    /// <summary>
    /// Generic infix parselet for a binary arithmetic operator; the only
    /// difference when parsing "+", "-", "*", "/", and "^" is precedence and
    /// associativity, so we can use a single parselet class for all of those.
    /// </summary>
    public class BinaryOperatorParselet : IInfixParselet
    {
        private readonly TokenType _operator;
        private readonly Precedence _precedence;
        private readonly bool _right;

        public BinaryOperatorParselet(TokenType @operator, Precedence precedence, bool right)
        {
            _operator = @operator;
            _precedence = precedence;
            _right = right;
        }

        public IExpression Parse(Parser parser, IExpression left, Token token)
        {
            // To handle right-associative operators like "^", we allow a slightly
            // lower precedence when parsing the right-hand side. This will let a
            // parselet with the same precedence appear on the right, which will then
            // take *this* parselet's result as its left-hand argument.
            var right = parser.ParseExpression(_precedence - (_right ? 1 : 0));
            return new OperatorExpression(left, _operator, right);
        }

        public Precedence Precedence
        {
            get { return _precedence; }
        }
    }
}


  
