using System.Text;
using Pratt;

namespace pratt.Expressions
{
    /// <summary>
    /// A postfix unary arithmetic expression like "a!"
    /// </summary>
    public class PostfixExpression : ExpressionBase
    {
        private readonly IExpression _left;
        private readonly TokenType _operator;

        public PostfixExpression(IExpression left, TokenType @operator)
        {
            _left = left;
            _operator = @operator;
        }

        public override void Print(StringBuilder builder)
        {
            builder.Append("(");
            _left.Print(builder);
            builder.Append(_operator.Punctuator()).Append(")");
        }
    }
}

