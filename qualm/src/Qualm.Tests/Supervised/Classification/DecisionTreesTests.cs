using NUnit.Framework;
using Qualm.Supervised.Classification;

namespace Qualm.Tests.Supervised.Classification
{
    [TestFixture]
    public class DecisionTreesTests
    {
        [Test]
        public void Can_create_id3_tree()
        {
            var matrix = GetDataSet();

            var tree = DecisionTrees.ID3(matrix, new[] {"A", "B"});

            Assert.IsNotNull(tree);
        }

        private static Matrix GetDataSet()
        {
            return new Matrix(new Number[,]
                                  {
                                      { 1, 1, 1 },    // YES
                                      { 1, 1, 1 },    // YES
                                      { 1, 0, 0 },    // NO
                                      { 0, 1, 0 },    // NO
                                      { 0, 1, 0 },    // NO
                                  });
        }
    }
}
