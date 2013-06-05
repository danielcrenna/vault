using System.Text;
using Pratt;

namespace pratt.Expressions
{
    /// <summary>
    /// A binary arithmetic expression like "a + b" or "c ^ d"
    /// </summary>
    public class OperatorExpression : ExpressionBase
    {
        private readonly IExpression _left;
        private readonly TokenType _operator;
        private readonly IExpression _right;

        public OperatorExpression(IExpression left, TokenType @operator, IExpression right)
        {
            _left = left;
            _operator = @operator;
            _right = right;
        }

        public override void Print(StringBuilder builder)
        {
            builder.Append("(");
            _left.Print(builder);
            builder.Append(" ").Append(_operator.Punctuator()).Append(" ");
            _right.Print(builder);
            builder.Append(")");
        }
    }
}