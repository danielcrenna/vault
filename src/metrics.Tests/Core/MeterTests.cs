using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using metrics.Core;
using metrics.Reporting;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class MeterTests
    {
        [Test]
        public void Can_count()
        {
            var meter = Metrics.Meter(typeof(MeterTests), "Can_count", "test", TimeUnit.Seconds);
            meter.Mark(3);
            Assert.AreEqual(3, meter.Count);
        }

        [Test]
        public void Can_meter()
        {
            const int count = 100000;
            var block = new ManualResetEvent(false);
            var meter = Metrics.Meter(typeof(MeterTests), "Can_meter", "test", TimeUnit.Seconds);
            Assert.IsNotNull(meter);

            var i = 0;
            ThreadPool.QueueUserWorkItem(s => 
            {
                while (i < count)
                {
                    meter.Mark();
                    i++;
                }
                Thread.Sleep(5000); // Wait for at least one EWMA rate tick
                block.Set();
            });
            block.WaitOne();

            Assert.AreEqual(count, meter.Count);

            var oneMinuteRate = meter.OneMinuteRate;
            var fiveMinuteRate = meter.FiveMinuteRate;
            var fifteenMinuteRate = meter.FifteenMinuteRate;
            var meanRate = meter.MeanRate;

            Assert.IsTrue(oneMinuteRate > 0);
            Trace.WriteLine("One minute rate:" + meter.OneMinuteRate);

            Assert.IsTrue(fiveMinuteRate > 0);
            Trace.WriteLine("Five minute rate:" + meter.FiveMinuteRate);
            
            Assert.IsTrue(fifteenMinuteRate > 0);
            Trace.WriteLine("Fifteen minute rate:" + meter.FifteenMinuteRate);

            Assert.IsTrue(meanRate > 0);
        }
    }
}