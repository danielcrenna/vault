using System.Diagnostics;
using metrics.Support;
using NUnit.Framework;

namespace metrics.Tests.Support
{
    [TestFixture]
    public class VolatileDoubleTests
    {
        [Test]
        public void Can_add_through_wrapper()
        {
            var rate1 = 15.50;
            rate1 += (2 * 10 - rate1);
            Trace.WriteLine(rate1);

            VolatileDouble rate2 = 15.50;
            rate2 += (2*10 - rate2);
            Trace.WriteLine(rate2);

            Assert.AreEqual(rate1, (double)rate2);
        }
    }
}
