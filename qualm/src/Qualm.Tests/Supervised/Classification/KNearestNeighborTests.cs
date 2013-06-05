using NUnit.Framework;
using Qualm.Supervised.Classification;

namespace Qualm.Tests.Classification
{
    [TestFixture]
    public class KNearestNeighborTests
    {
        [Test]
        public void Can_classify_with_euclidean_distance()
        {
            string[] labels;
            var featureSpace = GetDataSet(out labels);

            var b = KNearestNeighbor.Classify(new Vector(0, 0), featureSpace, labels, 3);
            Assert.AreEqual("B", b);

            var a = KNearestNeighbor.Classify(new Vector(1.0, 1.0), featureSpace, labels, 3);
            Assert.AreEqual("A", a);
        }

        [Test]
        public void Can_classify_with_manhattan_distance()
        {
            string[] labels;
            var featureSpace = GetDataSet(out labels);

            var b = KNearestNeighbor.Classify(DistanceType.Manhattan, new Vector(0, 0), featureSpace, labels, 3);
            Assert.AreEqual("B", b);

            var a = KNearestNeighbor.Classify(DistanceType.Manhattan, new Vector(1.0, 1.0), featureSpace, labels, 3);
            Assert.AreEqual("A", a);
        }

        [Test]
        public void Can_classify_with_minkowski_distance()
        {
            string[] labels;
            var featureSpace = GetDataSet(out labels);

            var b = KNearestNeighbor.Classify(DistanceType.Minkowski, new Vector(0, 0), featureSpace, labels, 3);
            Assert.AreEqual("B", b);

            var a = KNearestNeighbor.Classify(DistanceType.Minkowski, new Vector(1.0, 1.0), featureSpace, labels, 3);
            Assert.AreEqual("A", a);
        }

        private static Matrix GetDataSet(out string[] labels)
        {
            var featureSpace = new Matrix(new Number[,]
                                              {
                                                  { 1.0, 1.1 }, // A
                                                  { 1.0, 1.0 }, // A
                                                  { 0.0, 0.0 }, // B
                                                  { 0.0, 0.1 }  // B
                                              });

            labels = new[] { "A", "A", "B", "B" };
            return featureSpace;
        }
    }
}
