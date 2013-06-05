using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("{ToString()}")]
    public class Rule : Node
    {
        public string Identifier { get; set; }
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return string.Format("Rule: {0} = {1}", Identifier, Expression);
        }
    }
}