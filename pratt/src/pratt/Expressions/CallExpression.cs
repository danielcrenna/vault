using System.Collections.Generic;
using System.Text;

namespace pratt.Expressions
{
    /// <summary>
    /// A function call like "a(b, c, d)"
    /// </summary>
    public class CallExpression : ExpressionBase
    {
        private readonly IExpression _function;
        private readonly List<IExpression> _args;

        public CallExpression(IExpression function, List<IExpression> args)
        {
            _function = function;
            _args = args;
        }

        public override void Print(StringBuilder sb)
        {
            _function.Print(sb);
            sb.Append("(");
            for (var i = 0; i < _args.Count; i++)
            {
                _args[i].Print(sb);
                if (i < _args.Count - 1) sb.Append(", ");
            }
            sb.Append(")");
        }
    }
}
