using NUnit.Framework;

namespace Qualm.Tests
{
    [TestFixture]
    public class DisorderTests
    {
        [Test]
        public void Can_calculate_pure_entropy()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var entropy = Disorder.ShannonEntropy(matrix);

            Assert.IsNotNull(entropy);
            Assert.AreEqual((Number)0, entropy);
        }

        [Test]
        public void Can_calculate_entropy()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 1, 1, 0.5 },    // MAYBE
                                                  { 1, 1, 1 },      // YES
                                                  { 1, 0, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var entropy = Disorder.ShannonEntropy(matrix);

            Assert.IsNotNull(entropy);
            Assert.AreEqual((Number)1.3709505944546687, entropy);
        }

        [Test]
        public void Can_calculate_gini_impurity()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 1, 1, 0.5 },    // MAYBE
                                                  { 1, 1, 1 },      // YES
                                                  { 1, 0, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var impurity = Disorder.GiniImpurity(matrix);

            Assert.IsNotNull(impurity);
            Assert.AreEqual((Number)0.56, impurity);
        }

        [Test]
        public void Can_calculate_pure_gini_impurity()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var impurity = Disorder.GiniImpurity(matrix);

            Assert.IsNotNull(impurity);
            Assert.AreEqual((Number)0, impurity);
        }

        [Test]
        public void Can_calculate_pure_classification_error()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var impurity = Disorder.ClassificationError(matrix);

            Assert.IsNotNull(impurity);
            Assert.AreEqual((Number)0, impurity);
        }

        [Test]
        public void Can_calculate_classification_error()
        {
            var matrix = new Matrix(new Number[,]
                                              {
                                                  { 1, 1, 0.5 },    // MAYBE
                                                  { 1, 1, 1 },      // YES
                                                  { 1, 0, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                                  { 0, 1, 0 },      // NO
                                              });

            var impurity = Disorder.ClassificationError(matrix);

            Assert.IsNotNull(impurity);
            Assert.AreEqual((Number)0.8, impurity);
        }
    }
}
