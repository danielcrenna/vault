using System;
using System.Linq;
using NUnit.Framework;

namespace Dates.Tests
{
    [TestFixture]
    public class PeriodTests
    {
        [Test]
        public void Dates_fall_on_weekend()
        {
            var start = new DateTime(2009, 09, 05);
            var end = new DateTime(2010, 09, 05);
            var period = DatePeriod.Weekly;
            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }
            Assert.AreEqual(52, occurrences.Count());
        }

        [Test]
        public void Weekly_occurrences()
        {
            var start = new DateTime(2009, 09, 01);
            var end = new DateTime(2010, 09, 01);
            var period = DatePeriod.Weekly;
            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }
            Assert.AreEqual(52, occurrences.Count());
        }

        [Test]
        public void Hourly_occurrences()
        {
            var start = new DateTime(2009, 09, 05);
            var end = new DateTime(2009, 09, 06);
            var period = DatePeriod.Hourly;
            var occurrences = period.GetOccurrences(start, end);
            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }
            Assert.AreEqual(24, occurrences.Count());
        }

        [Test]
        public void Minutely_occurrences()
        {
            var start = new DateTime(2009, 09, 05, 1, 0, 0);
            var end = new DateTime(2009, 09, 05, 2, 0, 0);
            var period = new DatePeriod(DatePeriodFrequency.Minutes, 1);
            var occurrences = period.GetOccurrences(start, end);
            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }
            Assert.AreEqual(60, occurrences.Count());
        }

        [Test]
        public void Secondly_occurrences()
        {
            var start = new DateTime(2009, 09, 05, 1, 0, 0);
            var end = new DateTime(2009, 09, 05, 1, 1, 0);
            var period = new DatePeriod(DatePeriodFrequency.Seconds, 1);
            var occurrences = period.GetOccurrences(start, end);
            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }
            Assert.AreEqual(60, occurrences.Count());
        }
    }
}