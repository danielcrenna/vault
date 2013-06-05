using System.Text;
using Pratt;

namespace pratt.Expressions
{
    public class PrefixExpression : ExpressionBase
    {
        private readonly TokenType _operator;
        private readonly IExpression _right;

        public PrefixExpression(TokenType @operator, IExpression right)
        {
            _operator = @operator;
            _right = right;
        }

        public override void Print(StringBuilder sb)
        {
            sb.Append("(").Append(_operator.Punctuator());
            _right.Print(sb);
            sb.Append(")");
        }
    }
}