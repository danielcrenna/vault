using System;
using System.Collections.Generic;
using System.Linq;

namespace Qualm
{
    /// <summary>
    /// Contains measurements for computing disorder (noise, instability, etc.) in a data set.
    /// Disorder algorithms are used typically to find "information gain", to inform decision trees
    /// where they should branch based on improvements to disorder in the new branch.
    /// </summary>
    public class Disorder
    {
        /// <summary>
        /// Calculates the Shannon entropy of a data set
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class), find the entropy."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Entropy_(information_theory)" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static Number ShannonEntropy(ICollection<Number[]> dataSet)
        {
            var counts = GetCounts(dataSet);

            return ShannonEntropy(dataSet, counts);
        }

        /// <summary>
        /// Calculates the Shannon entropy of a matrix
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class), find the entropy."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Entropy_(information_theory)" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Number ShannonEntropy(Matrix matrix)
        {
            var dataSet = MatrixToDataSet(matrix);

            var counts = GetCounts(dataSet);

            return ShannonEntropy(dataSet, counts);
        }

        private static Number ShannonEntropy(ICollection<Number[]> rows, Dictionary<Number, int> counts)
        {
            var entropy = 0.0;

            var length = rows.Count;

            foreach (var item in counts)
            {
                var probability = (Number)item.Value / length;
                entropy -= probability * Math.Log(probability, 2);
            }

            return entropy;
        }

        
        /// <summary>
        /// Calculates the Gini Impurity of a data set
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class),
        ///  find the probability that for any future row, an outcome could be incorrectly classified."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Decision_tree_learning" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static Number GiniImpurity(ICollection<Number[]> dataSet)
        {
            var counts = GetCounts(dataSet);

            return GiniImpurity(dataSet, counts);
        }

        /// <summary>
        /// Calculates the Gini Impurity of a matrix
        /// </summary>
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class),
        ///  find the probability that for any future row, an outcome could be incorrectly classified."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Decision_tree_learning" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Number GiniImpurity(Matrix matrix)
        {
            var dataSet = MatrixToDataSet(matrix);

            var counts = GetCounts(dataSet);

            return GiniImpurity(dataSet, counts);
        }

        private static Number GiniImpurity(ICollection<Number[]> dataSet, Dictionary<Number, int> counts)
        {
            // 1 – (p1^2 + p2^2 + p3^2 ... pn^2)

            var index = 0.0;

            var length = dataSet.Count;

            foreach (var item in counts)
            {
                var probability = (Number)item.Value / length;

                index += Math.Pow(probability, 2);
            }

            return 1 - index;
        }

        /// <summary>
        /// Calculates the classification error of a data set
        /// </summary>
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class),
        ///  find the highest probability that for any future row, an outcome could be incorrectly classified."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Decision_tree_learning" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static Number ClassificationError(ICollection<Number[]> dataSet)
        {
            var counts = GetCounts(dataSet);

            return ClassificationError(dataSet, counts);
        }

        /// <summary>
        /// Calculates the classification error of a matrix
        /// </summary>
        /// <remarks>
        /// "Given a collection of data rows, where the final data point in a row is the outcome (or class),
        ///  find the highest probability that for any future row, an outcome could be incorrectly classified."
        /// </remarks>
        /// <see href="http://en.wikipedia.org/wiki/Decision_tree_learning" />
        /// <seealso href="http://people.revoledu.com/kardi/tutorial/DecisionTree/how-to-measure-impurity.htm" />
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Number ClassificationError(Matrix matrix)
        {
            var dataSet = MatrixToDataSet(matrix);

            var counts = GetCounts(dataSet);

            return ClassificationError(dataSet, counts);
        }

        private static Number ClassificationError(ICollection<Number[]> dataSet, Dictionary<Number, int> counts)
        {
            // 1 – Max{ p1, p2, p3, ... , pn } 
            
            var length = dataSet.Count;

            var probabilities = new List<Number>(counts.Count);

            foreach (var item in counts)
            {
                var probability = (Number)item.Value / length;

                probabilities.Add(probability);
            }

            return 1 - (probabilities.Max());
        }

        private static List<Number[]> MatrixToDataSet(Matrix dataSet)
        {
            var rows = new List<Number[]>();

            for (var i = 0; i < dataSet.Rows; i++)
            {
                var row = dataSet[i];
                rows.Add(row);
            }
            return rows;
        }

        private static Dictionary<Number, int> GetCounts(IEnumerable<Number[]> dataSet)
        {
            var labels = dataSet.Select(row => row.Last()).ToList();

            var counts = new Dictionary<Number, int>(labels.Count);

            foreach (var label in labels)
            {
                if (counts.ContainsKey(label))
                {
                    counts[label]++;
                }
                else
                {
                    counts.Add(label, 1);
                }
            }
            return counts;
        }
    }
}
