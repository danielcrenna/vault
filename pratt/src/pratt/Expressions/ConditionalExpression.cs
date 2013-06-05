using System.Text;

namespace pratt.Expressions
{
    /// <summary>
    /// A ternary conditional expression like "a ? b : c"
    /// </summary>
    public class ConditionalExpression : ExpressionBase
    {
        private readonly IExpression _condition;
        private readonly IExpression _then;
        private readonly IExpression _else;

        public ConditionalExpression(IExpression condition, IExpression then, IExpression @else)
        {
            _condition = condition;
            _then = then;
            _else = @else;
        }

        public override void Print(StringBuilder sb)
        {
            sb.Append("(");
            _condition.Print(sb);
            sb.Append(" ? ");
            _then.Print(sb);
            sb.Append(" : ");
            _else.Print(sb);
            sb.Append(")");
        }
    }
}
