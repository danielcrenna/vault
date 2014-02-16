using System;
using NUnit.Framework;
using metrics.Core;
using metrics.Reporting;
using metrics.Tests.Core;

namespace metrics.Tests.Reporting
{
    [TestFixture]
    public class JsonReportFormatterTests
    {
        [Test]
        public void Can_serialize_metrics_with_changes()
        {
            var metrics = new Metrics();
 
            var name = new MetricName(typeof(MeterTests), "Can_serialize_metrics_with_changes");
            var meter = metrics.Meter(typeof(MeterTests), "Can_serialize_metrics_with_changes", "test", TimeUnit.Seconds);
            Assert.IsNotNull(metrics.All[name], "Metric not found in central registry");

            meter.Mark(3);

            var reporter = new JsonReportFormatter();
            var json = reporter.GetSample();
            Console.WriteLine(json);
        }
    }
}
