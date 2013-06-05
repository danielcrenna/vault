using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Identifier: {Value}")]
    public class Identifier : Factor
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}