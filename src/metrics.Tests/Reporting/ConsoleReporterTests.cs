using System;
using System.Collections.Generic;
using metrics.Reporting;
using metrics.Tests.Core;
using NUnit.Framework;

namespace metrics.Tests.Reporting
{
    [TestFixture]
    public class ConsoleReporterTests
    {
        [Test]
        public void Can_run_with_known_counters()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_run_with_known_counters_counter");
            counter.Increment(100);

            var queue = new Queue<int>();
            Metrics.Gauge(typeof(GaugeTests), "Can_run_with_known_counters_gauge", () => queue.Count);
            queue.Enqueue(1);
            queue.Enqueue(2);

            var reporter = new ConsoleReporter(Console.Out);
            reporter.Run();
        }
    }
}
