using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Expression: {ToString()}")]
    public class Expression : Node
    {
        public IList<Node> Nodes
        {
            get
            {
                var nodes = new List<Node>();
                for (var i = 0; i < Terms.Count; i++)
                {
                    nodes.Add(Terms[i]);
                    if (i < Operators.Count)
                    {
                        nodes.Add(Operators[i]);
                    }
                }
                return nodes;
            }
        }

        public IList<Term> Terms { get; private set; }
        public IList<Operator> Operators { get; private set; }

        public Expression()
        {
            Terms = new List<Term>();
            Operators = new List<Operator>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var count = 0;
            foreach (var factor in Terms)
            {
                sb.Append(factor.ToString());
                count++;
                if (count < Terms.Count)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }
}

