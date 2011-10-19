using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using metrics.Reporting;
using metrics.Tests.Core;

namespace metrics.Tests.Reporting
{
    [TestFixture]
    public class FileReporterTests
    {
        private string _filename;

        [SetUp]
        public void Setup()
        {
            _filename = Path.GetTempFileName();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            if (File.Exists(_filename))
            {
                File.Delete(_filename);
            }
        }

        [Test]
        public void File_is_created_with_human_readable_content()
        {
            RegisterMetrics();

            using (var reporter = new FileReporter(_filename))
            {
                reporter.Run();
                Assert.IsTrue(File.Exists(_filename));
            }

            var contents = File.ReadAllText(_filename);
            Console.WriteLine(contents);
        }

        [Test]
        public void File_is_created_with_json_content()
        {
            RegisterMetrics();

            using (var reporter = new FileReporter(_filename, new JsonReportFormatter()))
            {
                reporter.Run();
                Assert.IsTrue(File.Exists(_filename));
            }

            var contents = File.ReadAllText(_filename);
            Console.WriteLine(contents);
        }

        [Test]
        public void Can_run_with_known_counters()
        {
            RegisterMetrics();

            using (var reporter = new FileReporter(_filename))
            {
                reporter.Run();
            }
        }

        [Test]
        public void Can_run_in_background()
        {
            const int ticks = 3;
            var block = new ManualResetEvent(false);

            RegisterMetrics();

            ThreadPool.QueueUserWorkItem(
                s =>
                {
                    using (var reporter = new FileReporter(_filename))
                    {
                        reporter.Start(3, TimeUnit.Seconds);
                        while (true)
                        {
                            Thread.Sleep(1000);
                            var runs = reporter.Runs;
                            if (runs == ticks)
                            {
                                block.Set();
                            }
                        }
                    }
                });

            block.WaitOne(TimeSpan.FromSeconds(5));
        }

        private static void RegisterMetrics()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_run_with_known_counters_counter");
            counter.Increment(100);

            var queue = new Queue<int>();
            Metrics.Gauge(typeof(GaugeTests), "Can_run_with_known_counters_gauge", () => queue.Count);
            queue.Enqueue(1);
            queue.Enqueue(2);
        }
    }
}
