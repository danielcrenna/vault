using System.Diagnostics;
using metrics.Core;
using NUnit.Framework;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class CLRProfilerTests
    {
        [Test]
        public void Can_get_heap_usage()
        {
            var heap = CLRProfiler.HeapUsage;
            Assert.IsNotNull(heap);
            Trace.WriteLine(heap);
        }

        [Test]
        public void Can_get_uptime()
        {
            var heap = CLRProfiler.Uptime;
            Assert.IsNotNull(heap);
            Trace.WriteLine(heap);
        }
    }
}
