using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Term: {ToString()}")]
    public class Term : Node
    {
        public ICollection<Factor> Factors { get; private set; }
        
        public Term()
        {
            Factors = new List<Factor>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var count = 0;
            foreach(var factor in Factors)
            {
                sb.Append(factor.ToString());
                count++;
                if(count < Factors.Count)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }
}