using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Qualm.Supervised.Classification
{
    [DebuggerDisplay("Node:{Label}")]
    public class DecisionTreeNode : ITreeNode
    {
        private readonly Matrix _matrix;

        private readonly List<ITreeNode> _children;

        /// <summary>
        /// The outcome, or target variable for this decision tree node.
        /// The outcome is the most frequent value present in the final
        /// column of the matrix the tree is built on. 
        /// </summary>
        public Number Outcome { get; set; }

        /// <summary>
        /// The feature label associated with this node
        /// </summary>
        public string Label { get; set; }

        public ICollection<ITreeNode> Children
        {
            get { return _children; }
        }

        public DecisionTreeNode(Matrix matrix)
        {
            Outcome = matrix.GetMajorityOutcome();
            _children = new List<ITreeNode>(0);
            _matrix = matrix;
        }

        public DecisionTreeNode(Matrix matrix, Number outcome)
        {
            Outcome = outcome;
            _children = new List<ITreeNode>(0);
            _matrix = matrix;
        }

        public void AddChild(ITreeNode child)
        {
            _children.Add(child);
        }

        /// <summary>
        /// Calculates a sorted list of information gains by splitting
        /// on each axes and value and measuring the resulting disorder
        /// </summary>
        /// <param name="disorderType">The disorder calculation algorithm to use</param>
        /// <returns></returns>
        public IDictionary<int, Number> GetInformationGain(DisorderType disorderType)
        {
            var grid = new Dictionary<int, Number>(_matrix.Rows);

            var baseline = GetDisorder(disorderType, _matrix);

            var axes = _matrix.Columns;

            for (var i = 0; i < axes; i++)
            {
                var row = _matrix[i];
                var values = row.Distinct();

                var disorder = 0.0;

                foreach(var value in values)
                {
                    var split = Split(i, value);
                    var probability = split.Rows / (float)_matrix.Rows;
                    disorder += probability * GetDisorder(disorderType, split);
                }

                var gain = baseline - disorder;
                grid.Add(i, gain);
            }

            var sorted = grid.OrderBy(kv => kv.Value).ToDictionary(x => x.Key, x => x.Value); ;
            
            return sorted;
        }

        /// <summary>
        /// Calculates the disorder of given matrix of values
        /// </summary>
        /// <param name="disorderType">The disorder calculation algorithm to use</param>
        /// <param name="matrix">The data set</param>
        /// <returns></returns>
        public static Number GetDisorder(DisorderType disorderType, Matrix matrix)
        {
            Number baseline;

            switch (disorderType)
            {
                case DisorderType.ShannonEntropy:
                    baseline = Disorder.ShannonEntropy(matrix);
                    break;
                case DisorderType.GiniImpurity:
                    baseline = Disorder.GiniImpurity(matrix);
                    break;
                case DisorderType.ClassificationError:
                    baseline = Disorder.ClassificationError(matrix);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("disorderType");
            }
            return baseline;
        }

        /// <summary>
        /// Splits a decision tree along a specified axis and value.
        /// </summary>
        /// <param name="axis">The axis column to split on</param>
        /// <param name="value">The value along the vector to split on</param>
        /// <returns></returns>
        public Matrix Split(int axis, Number value)
        {
            var list = new List<Number[]>();

            foreach(var vector in _matrix)
            {
                if (vector[axis] != value)
                {
                    continue;
                }
                
                var reduced = vector.ToList();
                reduced.RemoveAt(axis);

                var length = vector.Length - axis - 1;
                var segment = new ArraySegment<Number>(reduced.ToArray(), axis, length);

                list.Add(segment.Array);
            }

            return new Matrix(list);
        }

        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{{\"{0}\": ", Label);

            var array = Children.ToArray();

            if(array.Count() > 0)
            {
                sb.Append("{");
            }

            for (var i = 0; i < array.Length; i++)
            {
                var child = array[i];
                sb.Append("\"" + i + "\"").Append(": ");
                sb.Append(child.ToJson());
                if(i < array.Length -1)
                {
                    sb.Append(", ");
                }
            }

            if(array.Count() >0)
            {
                sb.Append("}");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}