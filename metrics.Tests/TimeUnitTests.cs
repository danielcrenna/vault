using System.Diagnostics;
using NUnit.Framework;

namespace metrics.Tests
{
    [TestFixture]
    public class TimeUnitTests
    {
        [Test]
        public void Can_convert_nanoseconds_to_nanoseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Nanoseconds.ToNanos(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(1000L, actual);
        }

        [Test]
        public void Can_convert_nanoseconds_to_microseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Nanoseconds.ToMicros(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(1L, actual);
        }

        [Test]
        public void Can_convert_nanoseconds_to_milliseconds()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Nanoseconds.ToMillis(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(10, actual);
        }

        [Test]
        public void Can_convert_nanoseconds_to_seconds()
        {
            const long expected = 10000000000;
            var actual = TimeUnit.Nanoseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(10, actual);
        }

        [Test]
        public void Can_convert_milliseconds_to_nanoseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToNanos(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(1000000000L, actual);
        }

        [Test]
        public void Can_convert_milliseconds_to_microseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToMicros(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(1000000L, actual);
        }

        [Test]
        public void Can_convert_milliseconds_to_milliseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToMillis(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(1000L, actual);
        }

        [Test]
        public void Can_convert_milliseconds_to_seconds()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(10000, actual);
        }

        [Test]
        public void Can_convert_milliseconds_to_minutes()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToMinutes(expected);
            Trace.WriteLine(actual);
            Assert.AreEqual(166, actual);
        }
    }
}
