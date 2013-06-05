using System.Text;

namespace pratt.Expressions
{
    /// <summary>
    /// A simple variable name expression like "abc"
    /// </summary>
    public class NameExpression : ExpressionBase
    {
        public NameExpression(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public override void Print(StringBuilder sb)
        {
            sb.Append(Name);
        }
    }
}