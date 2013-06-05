using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Repetition: {Expression}")]
    public class Repetition : Factor
    {
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return "{" + Expression + "}";
        }
    }
}