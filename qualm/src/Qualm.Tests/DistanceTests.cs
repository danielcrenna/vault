using NUnit.Framework;

namespace Qualm.Tests
{
    [TestFixture]
    public class DistanceTests
    {
        [Test]
        public void Can_calculate_hanning_distance()
        {
            // No error, no distance
            Assert.AreEqual(0, (int)Distance.Hanning(new Number[] { 1, 2, 3}, new Number[] {1, 2, 3}));

            // 1011101 and 1001001 is 2.
            Assert.AreEqual(2, (int)Distance.Hanning(new Number[] { 1, 0, 1, 1, 1, 0, 1 }, new Number[] { 1, 0, 0, 1, 0, 0, 1 }));

            // 2173896 and 2233796 is 3.
            Assert.AreEqual(3, (int)Distance.Hanning(new Number[] { 2, 1, 7, 3, 8, 9, 6 }, new Number[] { 2, 2, 3, 3, 7, 9, 6 }));
        }
    }
}
