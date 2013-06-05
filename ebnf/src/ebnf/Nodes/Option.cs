using System.Diagnostics;

namespace ebnf.Nodes
{
     [DebuggerDisplay("Option: [{Expression}]")]
    public class Option : Factor
    {
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return "[" + Expression + "]";
        }
    }
}