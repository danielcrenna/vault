using System.Collections.Generic;
using System.Linq;

namespace Qualm.Supervised.Classification
{
    public class DecisionTrees : IClassifier
    {
        /// <summary>
        /// Builds a tree using the Iterative Dichotimiser 3 (ID3) algorithm 
        /// <see href="http://en.wikipedia.org/wiki/ID3_algorithm" />
        /// </summary>
        /// <returns></returns>
        public static ITreeNode ID3(Matrix dataSet, IList<string> labels, DisorderType disorderType)
        {
            return CreateTree(dataSet, labels, disorderType);
        }

        public static ITreeNode ID3(Matrix dataSet, IList<string> labels)
        {
            return CreateTree(dataSet, labels, DisorderType.ShannonEntropy);
        }

        private static ITreeNode CreateTree(Matrix dataSet, IList<string> labels, DisorderType disorderType)
        {
            var outcomes = dataSet.GetOutcomes();

            if(outcomes.Distinct().Count().Equals(1))
            {
                return new DecisionTreeLeafNode(outcomes[0]);
            }

            var outcome = dataSet.GetMajorityOutcome();

            if(dataSet.Columns == 1)
            {
                return new DecisionTreeLeafNode(outcome);
            }

            ITreeNode node = new DecisionTreeNode(dataSet, outcome);

            var casted = (DecisionTreeNode) node;

            var best = casted.GetInformationGain(disorderType).First();

            var label = labels[best.Key];

            casted.Label = label;

            labels = labels.Where(l => !l.Equals(label)).ToList();

            var subset = dataSet.Select(row => row[best.Key]).ToList();
            
            var values = subset.Distinct().ToList();

            foreach(var value in values)
            {
                var copy = new List<string>(labels);
                var split = casted.Split(best.Key, value);
                var child = CreateTree(split, copy, disorderType);
                casted.AddChild(child);
            }

            ReverseNodeOrder(casted);

            return node;
        }

        private static void ReverseNodeOrder(DecisionTreeNode casted)
        {
            var reversed = casted.Children.Reverse().ToList();
            casted.Children.Clear();
            foreach(var item in reversed)
            {
                casted.Children.Add(item);
            }
        }
    }
}
