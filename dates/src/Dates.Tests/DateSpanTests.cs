using System;
using NUnit.Framework;

namespace Dates.Tests
{
    [TestFixture]
    public class DateSpanTests
    {
        [Test]
        public void Can_get_date_difference_in_days()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(5);
            var diff = DateSpan.GetDifference(DateInterval.Days, start, end);

            Assert.AreEqual(5, diff);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_one_month()
        {
            var start = new DateTime(2009, 09, 30);
            var end = new DateTime(2009, 10, 01);

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            Assert.AreEqual(1, days);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_one_week()
        {
            var start = new DateTime(2009, 09, 30);
            var end = start.AddDays(10);

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            var weeks = DateSpan.GetDifference(DateInterval.Weeks, start, end);

            Assert.AreEqual(10, days);
            Assert.AreEqual(1, weeks);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_two_months()
        {
            var start = new DateTime(2009, 09, 30);
            var end = new DateTime(2009, 11, 04); // 4 days in November, 31 in October

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            Assert.AreEqual(35, days);
        }

        [Test]
        public void Can_handle_composite_spans()
        {
            var start = new DateTime(2009, 9, 30);
            var end = new DateTime(2009, 10, 31);
            var span = new DateSpan(start, end);

            Assert.AreEqual(1, span.Months);
            Assert.AreEqual(1, span.Days);

            Console.WriteLine(span.Months);
            Console.WriteLine(span.Days);

            var difference = DateSpan.GetDifference(DateInterval.Days, start, end);
            Console.WriteLine(difference);
        }
    }
}