using System;
using System.Collections.Generic;
using System.Linq;

namespace Qualm.Supervised.Classification
{
    /// <summary>
    /// An <see cref="IClassifier"/> that measures distances from the input to the training data.
    /// <see href="http://en.wikipedia.org/wiki/K-nearest_neighbor_algorithm" />
    /// </summary>
    public class KNearestNeighbor : IClassifier
    {
        /// <summary>
        /// Given an input feature, a feature space and its associated labels, and a positive integer 'k',
        /// Determines the 'k' nearest neighbor label for the input feature. The 'k' value corresponds
        /// to the number of nearest neighbors to use in the voting process.
        /// 
        /// <remarks> 
        /// "When I have this grid of data points, and I provide one additional example row, find the 'k' number
        /// of rows that are most similar, count up the number of occurrences of each label for each row (1 to 'k'), 
        /// and choose the label with the highest occurrence."
        /// </remarks> 
        /// <see href="http://en.wikipedia.org/wiki/K-nearest_neighbor_algorithm" />
        /// </summary>
        /// <param name="distanceType">The type of equation to use when measuring the distance between each data point</param>
        /// <param name="input">The matrix row to input; must have the same number of columns as the feature space</param>
        /// <param name="featureSpace">The feature space matrix; everything we know</param>
        /// <param name="labels">The results for each feature space row; what we call each collection of data points</param>
        /// <param name="k">The number of nearest neighbors to include in the voting; the label with the most occurrences in 'k' neighbors wins</param>
        /// <returns></returns>
        public static string Classify(DistanceType distanceType, Number[] input, Matrix featureSpace, IList<string> labels, int k)
        {
            if (labels.Count() != featureSpace.Rows)
            {
                throw new ArgumentException("The number of labels must match the number of rows of data in the feature space", "labels");
            }

            var distances = CalculateDistances(distanceType, featureSpace, input);

            var nearestNeighbors = distances.OrderByDescending(d => d.Value).Take(k);

            var votes = new Dictionary<string, int>(k);

            foreach (var label in nearestNeighbors.Select(neighbor => labels[neighbor.Key]))
            {
                if (votes.ContainsKey(label))
                {
                    votes[label]++;
                }
                else
                {
                    votes.Add(label, 1);
                }
            }

            var nearest = votes.OrderByDescending(v => v.Value).First().Key;

            return nearest;
        }

        /// <summary>
        /// Given an input feature, a feature space and its associated labels, and a positive integer 'k',
        /// Determines the 'k' nearest neighbor label for the input feature. The 'k' value corresponds
        /// to the number of nearest neighbors to use in the voting process.
        /// 
        /// <remarks> 
        /// "When I have this grid of data points, and I provide one additional example row, find the 'k' number
        /// of rows that are most similar, count up the number of occurrences of each label for each row (1 to 'k'), 
        /// and choose the label with the highest occurrence."
        /// </remarks> 
        /// <see href="http://en.wikipedia.org/wiki/K-nearest_neighbor_algorithm" />
        /// </summary>
        /// <param name="input">The matrix row to input; must have the same number of columns as the feature space</param>
        /// <param name="featureSpace">The feature space matrix; everything we know</param>
        /// <param name="labels">The results for each feature space row; what we call each collection of data points</param>
        /// <param name="k">The number of nearest neighbors to include in the voting; the label with the most occurrences in 'k' neighbors wins</param>
        /// <returns></returns>
        public static string Classify(Number[] input, Matrix featureSpace, IList<string> labels, int k)
        {
            return Classify(DistanceType.Euclidean, input, featureSpace, labels, k);
        }

        /// <summary>
        /// Given an input feature, a feature space and its associated labels, and a positive integer 'k',
        /// Determines the 'k' nearest neighbor label for the input feature. The 'k' value corresponds
        /// to the number of nearest neighbors to use in the voting process.
        /// 
        /// <remarks> 
        /// "When I have this grid of data points, and I provide one additional example row, find the 'k' number
        /// of rows that are most similar, count up the number of occurrences of each label for each row (1 to 'k'), 
        /// and choose the label with the highest occurrence."
        /// </remarks> 
        /// <see href="http://en.wikipedia.org/wiki/K-nearest_neighbor_algorithm" />
        /// </summary>
        /// <param name="input">The vector to input; must have the same number of columns as the feature space, in this case just two</param>
        /// <param name="featureSpace">The feature space matrix; everything we know</param>
        /// <param name="labels">The results for each feature space row; what we call each collection of data points</param>
        /// <param name="k">The number of nearest neighbors to include in the voting; the label with the most occurrences in 'k' neighbors wins</param>
        /// <returns></returns>
        public static string Classify(Vector input, Matrix featureSpace, IList<string> labels, int k)
        {
            return Classify(DistanceType.Euclidean, new[] { input.X, input.Y }, featureSpace, labels, k);
        }

        /// <summary>
        /// Given an input feature, a feature space and its associated labels, and a positive integer 'k',
        /// Determines the 'k' nearest neighbor label for the input feature. The 'k' value corresponds
        /// to the number of nearest neighbors to use in the voting process.
        /// 
        /// <remarks> 
        /// "When I have this grid of data points, and I provide one additional example row, find the 'k' number
        /// of rows that are most similar, count up the number of occurrences of each label for each row (1 to 'k'), 
        /// and choose the label with the highest occurrence."
        /// </remarks> 
        /// <see href="http://en.wikipedia.org/wiki/K-nearest_neighbor_algorithm" />
        /// </summary>
        /// <param name="distanceType">The type of equation to use when measuring the distance between each data point</param>
        /// <param name="input">The vector to input; must have the same number of columns as the feature space, in this case just two</param>
        /// <param name="featureSpace">The feature space matrix; everything we know</param>
        /// <param name="labels">The results for each feature space row; what we call each collection of data points</param>
        /// <param name="k">The number of nearest neighbors to include in the voting; the label with the most occurrences in 'k' neighbors wins</param>
        /// <returns></returns>
        public static string Classify(DistanceType distanceType, Vector input, Matrix featureSpace, IList<string> labels, int k)
        {
            return Classify(distanceType, new[] { input.X, input.Y }, featureSpace, labels, k);
        }

        private static Dictionary<int, Number> CalculateDistances(DistanceType distanceType, Matrix featureSpace, Number[] input)
        {
            var distances = new Dictionary<int, Number>(featureSpace.Rows);

            for (var i = 0; i < featureSpace.Rows; i++)
            {
                var features = featureSpace[i];

                Number distance = 0;

                switch (distanceType)
                {
                    case DistanceType.Euclidean:
                        distance = Distance.Euclidean(input, features);
                        break;
                    case DistanceType.Manhattan:
                        distance = Distance.Manhattan(input, features);
                        break;
                    case DistanceType.Minkowski:
                        distance = Distance.Minkowski(input, features);
                        break;
                    case DistanceType.Hanning:
                        distance = Distance.Hanning(input, features);
                        break;
                }

                distances.Add(i, distance);
            }

            return distances;
        }
    }
}
