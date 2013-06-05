using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Operator: {ToString()}")]
    public class Operator : Node
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}