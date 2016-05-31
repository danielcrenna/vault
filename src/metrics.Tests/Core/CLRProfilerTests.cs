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
            var value = CLRProfiler.GlobalTotalNumberOfContentions;
            AssertProfilerHasValue(value);

            value = CLRProfiler.GlobalContentionRatePerSecond;
            AssertProfilerHasValue(value);

            value = CLRProfiler.GlobalCurrentQueueLength;
            AssertProfilerHasValue(value);

            value = CLRProfiler.GlobalQueueLengthPeak;
            AssertProfilerHasValue(value);
        }

        [Test]
        public void Can_enumerate_machine_categories()
        {
            // http://technet.microsoft.com/en-us/library/cc768048.aspx
            EnumerateCountersFor("System");
            EnumerateCountersFor("Processor");
            EnumerateCountersFor("Memory");
            EnumerateCountersFor("Network Interface");
            EnumerateCountersFor("PhysicalDisk", "_Total");
            EnumerateCountersFor("LogicalDisk", "_Total");
        }
        
        [Test]
        [Ignore("This is a long-running test")]
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

            if(counterCategory.CategoryType == PerformanceCounterCategoryType.SingleInstance)
            {
                foreach (var counter in counterCategory.GetCounters())
                {
                    sb.AppendLine(string.Format("{0}:{1}", category, counter.CounterName));
                }
            }
            else
            {
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

        private static void EnumerateCounters()
        {
            var categories = PerformanceCounterCategory.GetCategories().Select(c => c.CategoryName).OrderBy(s => s).ToArray();

            var sb = new StringBuilder();

            foreach (var category in categories)
            {
                var counterCategory = new PerformanceCounterCategory(category);

                foreach (var counterInstance in counterCategory.GetInstanceNames())
                {
                    try
                    {
                        foreach (var counter in counterCategory.GetCounters(counterInstance))
                        {
                            sb.AppendLine(string.Format("{0}:{1}:{2}", counterInstance, category, counter.CounterName));
                        }
                    }
                    catch
                    {
                        // Drop it on the floor
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
