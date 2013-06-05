using Qualm.Serialization;

namespace Qualm.Supervised.Classification
{
    /// <summary>
    /// A generic node of a decision tree
    /// </summary>
    public interface ITreeNode : IJson
    {
        /// <summary>
        /// The outcome, or target variable for this decision tree node.
        /// The outcome is the most frequent value present in the final
        /// column of the matrix the tree is built on. 
        /// </summary>
        Number Outcome { get; set; }
    }
}