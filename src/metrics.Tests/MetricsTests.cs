using metrics.Core;
using metrics.Tests.Core;
using NUnit.Framework;

namespace metrics.Tests
{
    [TestFixture]
    public class MetricsTests
    {
        [SetUp]
        public void SetUp()
        {
            Metrics.Clear();
        }

        [Test]
        public void Can_get_same_metric_when_metric_exists()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.AreSame(counter, same);
        }

        [Test]
        public void Can_get_all_registered_metrics()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(same);

            Assert.AreEqual(1, Metrics.All.Count);
        }

        [Test]
        public void Can_get_all_registered_metrics_as_readonly()
        {
            var all = Metrics.All.Count;

            Assert.AreEqual(0, all);

            Metrics.All.Add(new MetricName(typeof(CounterTests), "No way this is going to get added"), new CounterMetric());

            Assert.AreEqual(0, all);
        }

        [Test]
        public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
            Assert.AreEqual(0, Metrics.All.Count);

            var name = new MetricName(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

            Metrics.Counter(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

            Assert.AreEqual(1, Metrics.All.Count);

            var value = Metrics.All[name];

            Assert.IsNotNull(value);

            ((CounterMetric)value).Increment();

            Assert.AreEqual(0, ((CounterMetric)Metrics.All[name]).Count);
        }
    }
}
