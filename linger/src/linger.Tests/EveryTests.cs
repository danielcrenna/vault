using System;
using System.Linq;
using NUnit.Framework;

namespace linger.Tests
{
    [TestFixture]
    public class EveryTests
    {
        [Test]
        public void Periods_with_undefined_endings_have_infinite_occurrences()
        {
            var job = new ScheduledJob();
            var info = new Every(job).Hour();
            Assert.Throws<ArgumentException>(() => { info.AllOccurrences.ToList(); });
            Assert.IsNull(info.LastOccurrence);

            var next = info.NextOccurrence;
            Assert.IsNotNull(next);
            Console.WriteLine(next);
        }

        [Test]
        public void Periods_with_defined_endings_have_finite_occurrences()
        {
            var job = new ScheduledJob();
            var info = new Every(job).Hour().For(6).Days();
            Assert.AreEqual(24 * 6, info.AllOccurrences.Count());

            Assert.IsNotNull(info.NextOccurrence);
            Assert.IsNotNull(info.LastOccurrence);
            Assert.AreEqual(info.NextOccurrence, info.AllOccurrences.First());
            Assert.AreEqual(info.LastOccurrence, info.AllOccurrences.Last());
        }
    }
}