using System.Collections.Generic;
using metrics.Core;
using NUnit.Framework;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class GaugeTests
    {
        [Test]
        public void Can_gauge_scalar_value()
        {
            var queue = new Queue<int>();
            var gauge = new GaugeMetric<int>(() => queue.Count);

            queue.Enqueue(5);
            Assert.AreEqual(1, gauge.Value);

            queue.Enqueue(6);
            queue.Dequeue();
            Assert.AreEqual(1, gauge.Value);

            queue.Dequeue();
            Assert.AreEqual(0, gauge.Value);
        }

        [Test]
        public void Can_use_gauge_metric()
        {
            var queue = new Queue<int>();
            var gauge = Metrics.Gauge(typeof(GaugeTests), "Can_use_gauge_metric", () => queue.Count);
            queue.Enqueue(5);
            Assert.AreEqual(1, gauge.Value);
        }
    }
}
