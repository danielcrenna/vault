using System.Text;

namespace pratt.Expressions
{
    /// <summary>
    /// An assignment expression like "a = b"
    /// </summary>
    public class AssignExpression : ExpressionBase
    {
        private readonly string _name;
        private readonly IExpression _right;

        public AssignExpression(string name, IExpression right)
        {
            _name = name;
            _right = right;
        }

        public override void Print(StringBuilder sb)
        {
            sb.Append("(").Append(_name).Append(" = ");
            _right.Print(sb);
            sb.Append(")");
        }
    }
}