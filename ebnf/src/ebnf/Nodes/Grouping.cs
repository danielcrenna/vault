using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Grouping: ({Expression})")]
    public class Grouping : Factor
    {
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return "(" + Expression + ")";
        }
    }
}