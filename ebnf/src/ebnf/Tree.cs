using System.Collections.Generic;
using ebnf.Nodes;

namespace ebnf
{
    public class Tree : Node
    {
        public IList<Error> Errors { get; private set; }
        public IList<Rule> Rules { get; private set; } 

        public Tree()
        {
            Errors = new List<Error>();
            Rules = new List<Rule>();
        }

        public void Add(Rule rule)
        {
            Rules.Add(rule);
        }
    }
}