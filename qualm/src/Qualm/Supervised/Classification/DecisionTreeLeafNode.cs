using System.Diagnostics;
using System.Text;

namespace Qualm.Supervised.Classification
{
    [DebuggerDisplay("Leaf:{Outcome._key}")]
    public class DecisionTreeLeafNode : ITreeLeafNode
    {
        /// <summary>
        /// The outcome, or target variable for this decision tree node.
        /// The outcome is the most frequent value present in the final
        /// column of the matrix the tree is built on. 
        /// </summary>
        public Number Outcome { get; set; }

        public DecisionTreeLeafNode(Number outcome)
        {
            Outcome = outcome;
        }
        
        public string ToJson()
        {
            var sb = new StringBuilder();

            sb.Append(((double)Outcome).ToString("0.00"));
            
            return sb.ToString();
        }
    }
}