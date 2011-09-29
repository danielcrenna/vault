using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using metrics.Core;
using NUnit.Framework;
using metrics.Util;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class CLRProfilerTests
    {
        [Test]
        public void Can_dump_tracked_threads()
        {
            var factory = new NamedThreadFactory("Can_dump_managed_threads");

            var thread = factory.New(() =>
                                         {
                                             while (true)
                                             {
                                                 Debug.Assert("This".Equals("This"));
                                             }
                                         });

            thread.Start();
            
            Console.WriteLine(CLRProfiler.DumpTrackedThreads());
        }

        [Test]
        public void Can_get_machine_metrics()
        {
            var value = CLRProfiler.MachineTotalContentions;
            AssertProfilerHasValue(value);

            value = CLRProfiler.MachineContentionRatePerSecond;
            AssertProfilerHasValue(value);

            value = CLRProfiler.MachineCurrentQueueLength;
            AssertProfilerHasValue(value);

            value = CLRProfiler.MachineQueueLengthPeak;
            AssertProfilerHasValue(value);
        }

        [Test]
        public void Can_enumerate_machine_categories()
        {
            EnumerateCountersFor(".NET CLR LocksAndThreads", "_Global_");
            EnumerateCountersFor("PhysicalDisk", "_Total");
            EnumerateCountersFor("Network Interface");
        }

        [Test]
        public void Can_enumerate_all_counters()
        {
            EnumerateCounters();
        }

        private static void EnumerateCountersFor(string category, string instance)
        {
            var sb = new StringBuilder();
            var counterCategory = new PerformanceCounterCategory(category);
            foreach (var counter in counterCategory.GetCounters(instance))
            {
                sb.AppendLine(string.Format("{0}:{1}:{2}", instance, category, counter.CounterName));
            }

            Console.WriteLine(sb.ToString());
        }

        private static void EnumerateCountersFor(string category)
        {
            var sb = new StringBuilder();
            var counterCategory = new PerformanceCounterCategory(category);

            foreach (var counterInstance in counterCategory.GetInstanceNames())
            {
                foreach (var counter in counterCategory.GetCounters(counterInstance))
                {
                    sb.AppendLine(string.Format("{0}:{1}:{2}", counterInstance, category, counter.CounterName));
                }
            }

            Console.WriteLine(sb.ToString());
        }

        private static void EnumerateCounters()
        {
            var categories = PerformanceCounterCategory.GetCategories().Select(c => c.CategoryName).OrderBy(s => s).ToArray();

            var sb = new StringBuilder();

            foreach (var category in categories)
            {
                var counterCategory = new PerformanceCounterCategory(category);

                foreach (var counterInstance in counterCategory.GetInstanceNames())
                {
                    foreach (var counter in counterCategory.GetCounters(counterInstance))
                    {
                        sb.AppendLine(string.Format("{0}:{1}:{2}", counterInstance, category, counter.CounterName));
                    }
                }
            }

            Console.WriteLine(sb.ToString());
        }
        
        private static void AssertProfilerHasValue(double heap)
        {
            Assert.IsNotNull(heap);
            Trace.WriteLine(heap);
        }
    }
}
