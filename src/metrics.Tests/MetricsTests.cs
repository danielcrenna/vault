using metrics.Core;
using metrics.Tests.Core;
using NUnit.Framework;

namespace metrics.Tests
{
    [TestFixture]
    public class MetricsTests
    {
        Metrics _metrics = new Metrics();
        [SetUp]
        public void SetUp()
        {
            _metrics.Clear();
        }

        [Test]
        public void Can_get_same_metric_when_metric_exists()
        {
           // var metrics = new Metrics();
            var counter = _metrics.Counter(typeof (CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = _metrics.Counter(typeof (CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.AreSame(counter, same);
        }

        [Test]
        public void Can_get_all_registered_metrics()
        {
            var counter = _metrics.Counter(typeof (CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = _metrics.Counter(typeof (CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(same);

            Assert.AreEqual(1, _metrics.All.Count);
        }

        [Test]
        public void Can_get_all_registered_metrics_as_readonly()
        {
            var all = _metrics.All.Count;

            Assert.AreEqual(0, all);

            _metrics.All.Add(new MetricName(typeof (CounterTests), "No way this is going to get added"),
                            new CounterMetric());

            Assert.AreEqual(0, all);
        }

        [Test]
        public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
            Assert.AreEqual(0, _metrics.All.Count);
            var name = new MetricName(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");
            _metrics.Counter(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");
            Assert.AreEqual(1, _metrics.All.Count);

            var value = _metrics.All[name];

            Assert.IsNotNull(value);
            ((CounterMetric) value).Increment();
            Assert.AreEqual(0, ((CounterMetric) _metrics.All[name]).Count);
        }

        [Test]
        public void Can_get_metrics_from_collection_with_registered_changes()
        {
            // Counter
            var name = new MetricName(typeof(MeterTests), "counter");
            var counter = _metrics.Counter(typeof (MeterTests), "counter");
            Assert.IsNotNull(_metrics.All[name], "Metric not found in central registry");
            counter.Increment(10);
            var actual = ((CounterMetric)_metrics.All[name]).Count;
            Assert.AreEqual(10, actual, "Immutable copy did not contain correct values for this metric");
            
            // Meter
            name = new MetricName(typeof(MeterTests), "meter");
            var meter = _metrics.Meter(typeof(MeterTests), "meter", "test", TimeUnit.Seconds);
            Assert.IsNotNull(_metrics.All[name], "Metric not found in central registry");
            meter.Mark(3);
            actual = ((MeterMetric)_metrics.All[name]).Count;
            Assert.AreEqual(3, actual, "Immutable copy did not contain correct values for this metric");
        }

        [Test]
        public void Can_safely_remove_metrics_from_outer_collection_without_affecting_registry()
        {
            var name = new MetricName(typeof(MeterTests), "Can_safely_remove_metrics_from_outer_collection_without_affecting_registry");
            var meter = _metrics.Meter(typeof(MeterTests), "Can_safely_remove_metrics_from_outer_collection_without_affecting_registry", "test", TimeUnit.Seconds);
            meter.Mark(3);

            _metrics.All.Remove(name);
            var metric = _metrics.All[name];
            Assert.IsNotNull(metric, "Metric was removed from central registry");
        }
    }
}
