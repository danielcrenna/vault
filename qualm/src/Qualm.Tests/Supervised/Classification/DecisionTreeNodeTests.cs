using System.Linq;
using NUnit.Framework;
using Qualm.Supervised.Classification;

namespace Qualm.Tests.Supervised.Classification
{
    [TestFixture]
    public class DecisionTreeNodeTests
    {
        [Test]
        public void Can_split_dataset()
        {
            var matrix = GetDataSet();
            
            var tree = new DecisionTreeNode(matrix);
            
            var result = tree.Split(0, 1);
            
            // { {1, 1}, {1, 1}, {0, 0} }
            Assert.IsNotNull(result);
            foreach(var row in result)
            {
                Assert.AreEqual(2, row.Length);
                Assert.AreEqual(row[0], row[1]);
            }
            
            result = tree.Split(0, 0);
            // { {1, 0}, {1, 0} }
            Assert.IsNotNull(result);
            foreach (var row in result)
            {
                Assert.AreEqual(2, row.Length);
                Assert.AreNotEqual(row[0], row[1]);
            } 
        }

        [Test]
        public void Can_get_gain_grid_with_shannon_entropy()
        {
            var matrix = GetDataSet();

            var tree = new DecisionTreeNode(matrix);

            var grid = tree.GetInformationGain(DisorderType.ShannonEntropy);

            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.Count);
            Assert.AreEqual(0, grid.First().Key);
            Assert.AreEqual(1, grid.Last().Key);
        }

        [Test]
        public void Can_get_gain_grid_with_gini_impurity()
        {
            var matrix = GetDataSet();
            
            var tree = new DecisionTreeNode(matrix);

            var grid = tree.GetInformationGain(DisorderType.GiniImpurity);

            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.Count);
            Assert.AreEqual(0, grid.First().Key);
            Assert.AreEqual(1, grid.Last().Key);
        }

        [Test]
        public void Can_get_gain_grid_with_classification_error()
        {
            var matrix = GetDataSet();
            
            var tree = new DecisionTreeNode(matrix);

            var grid = tree.GetInformationGain(DisorderType.ClassificationError);

            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.Count);
            Assert.AreEqual(1, grid.First().Key);
            Assert.AreEqual(2, grid.Last().Key);
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
