using System;
using System.Diagnostics;
using NUnit.Framework;

namespace linger.Tests
{
    [TestFixture]
    public class LingerTests : SqliteFixture
    {
        [Test]
        public void Job_is_executed_when_attempted()
        {
            Linger.DelayJobs = false;
            var success = Linger.QueueForExecution(new HelloWorldJob(), 0);
            Assert.IsTrue(success);
            Linger.DelayJobs = true;
        }

        [Test]
        public void Dumps_json_on_request()
        {
            Linger.DelayJobs = true;
            Linger.QueueForExecution(new HelloWorldJob(), runAt: DateTime.Now.AddYears(100));
            var json = Linger.Dump();
            Assert.IsNotNullOrEmpty(json);
            Trace.WriteLine(json);
        }

        [Test]
        public void Respects_repeat_information_when_present()
        {
            Linger.DelayJobs = true;
            Linger.QueueForExecution(new HelloWorldJob(), runAt: DateTime.Now.AddYears(100), repeat: job => job.Every().Day());
            var json = Linger.Dump();
            Assert.IsNotNullOrEmpty(json);
            Trace.WriteLine(json);
        }
    }
}