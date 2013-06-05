using System;
using System.Collections.Generic;
using System.Linq;

namespace Qualm
{
    /// <summary>
    /// A distance, also known as a metric, in Euclidean space.
    /// <see href="http://en.wikipedia.org/wiki/Metric_%28mathematics%29"/>
    /// </summary>
    public class Distance
    {
        /// <summary>
        /// Calculates the Euclidean distance between two sets of points.
        /// <see href="http://en.wikipedia.org/wiki/Euclidean_distance" />
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Number Euclidean(Number[] x, Number[] y)
        {
            CheckArgumentLengths(x, y);

            var sum = x.Select((n, i) => Math.Pow(Math.Abs(n - y[i]), 2)).Sum();

            var distance = Math.Sqrt(sum);

            return distance;
        }

        /// <summary>
        /// Calculates the Manhattan (taxicab) distance between two sets of points.
        /// <see href="http://en.wikipedia.org/wiki/Manhattan_distance"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Number Manhattan(Number[] x, Number[] y)
        {
            CheckArgumentLengths(x, y);

            var distance = x.Select((n, i) => Math.Abs(n - y[i])).Sum();

            return distance;
        }

        /// <summary>
        /// Calculates the Minkowski distance between two sets of points.
        /// <see href="http://en.wikipedia.org/wiki/Minkowski_distance" />
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Number Minkowski(Number[] x, Number[] y)
        {
            CheckArgumentLengths(x, y);
            
            var distance = x.Select((n, i) => Math.Abs(n - y[i])).Max();

            return distance;
        }

        /// <summary>
        /// Calculates the Hanning distance between two sets of points.
        /// <see href="http://en.wikipedia.org/wiki/Hamming_distance" />
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Number Hanning(Number[]x, Number[] y)
        {
            CheckArgumentLengths(x, y);

            return x.Zip(y, (l, r) => l.Equals(r) ? 0 : 1).Sum();
        }

        private static void CheckArgumentLengths(ICollection<Number> x, ICollection<Number> y)
        {
            if (x.Count != y.Count)
            {
                throw new ArgumentException("The number of elements in X must match the number of elements in Y", "x");
            }
        }
    }
}
