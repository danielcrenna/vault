using NUnit.Framework;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class TimerTests : MetricTestBase
    {
        [Test]
        public void Can_time_stuff()
        {
            var timer = Metrics.Timer()
        }
    }
}