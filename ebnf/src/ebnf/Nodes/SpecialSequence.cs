using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("SpecialSequence: {Expression}")]
    public class SpecialSequence : Factor
    {
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return "?" + Expression + "?";
        }
    }
}