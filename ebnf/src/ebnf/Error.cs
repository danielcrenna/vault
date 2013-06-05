using System.Diagnostics;

namespace ebnf
{
    [DebuggerDisplay("{ToString()}")]
    public class Error
    {
        public int Line { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("Error on line {0}, position {1}: {2}", Line, Position, Description);
        }
    }
}