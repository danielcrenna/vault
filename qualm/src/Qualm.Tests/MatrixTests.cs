using NUnit.Framework;

namespace Qualm.Tests
{
    [TestFixture]
    public class MatrixTests
    {
        [Test]
        public void Can_initialize_with_values()
        {
            var matrix = new Matrix(new Number[,]
                    {
                        { 1.0, 1.1 },
                        { 1.0, 1.0 },
                        { 0.0, 0.0 },
                        { 0.0, 0.1 }
                    });

            Assert.IsNotNull(matrix);
            Assert.AreEqual(4, matrix.Rows);
            Assert.AreEqual(2, matrix.Columns);
        }
    }
}
