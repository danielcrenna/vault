using System;
using NUnit.Framework;
using metrics.Stats;

namespace metrics.Tests.Stats
{
    [TestFixture]
    public class EWMATests
    {
        [Test]
        public void Can_relative_decay_rates_for_discrete_values()
        {
            var one = EWMA.OneMinuteEWMA();
            one.Update(100000);
            one.Tick();

            var five = EWMA.FiveMinuteEWMA();
            five.Update(100000);
            five.Tick();

            var fifteen = EWMA.FifteenMinuteEWMA();
            fifteen.Update(100000);
            fifteen.Tick();

            var rateOne = one.Rate(TimeUnit.Seconds);
            var rateFive = five.Rate(TimeUnit.Seconds);
            var rateFifteen = fifteen.Rate(TimeUnit.Seconds);

            Assert.AreEqual(20000, rateOne);
            Assert.AreEqual(20000, rateFive);
            Assert.AreEqual(20000, rateFifteen);

            ElapseMinute(one);
            rateOne = one.Rate(TimeUnit.Seconds);

            ElapseMinute(five);
            rateFive = five.Rate(TimeUnit.Seconds);

            ElapseMinute(fifteen);
            rateFifteen = fifteen.Rate(TimeUnit.Seconds);

            Assert.AreEqual(7357.5888234288504d, rateOne);
            Assert.AreEqual(16374.615061559636d, rateFive);
            Assert.AreEqual(18710.13970063235d, rateFifteen);
        }

        [Test]
        public void Can_retrieve_decaying_rate_with_discrete_value()
        {
            var ewma = EWMA.OneMinuteEWMA();
            ewma.Update(3);
            ewma.Tick();    // Assumes 5 seconds have passed

            var rate = ewma.Rate(TimeUnit.Seconds);
            Assert.AreEqual(rate, 0.6, "the EWMA has a rate of 0.6 events/sec after the first tick");

            ElapseMinute(ewma);
            
            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.22072766, rate, 8, "the EWMA has a rate of 0.22072766 events/sec after 1 minute");

            ElapseMinute(ewma);
            
            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.08120116, rate, 8, "the EWMA has a rate of 0.08120116 events/sec after 2 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.02987224, rate, 8, "the EWMA has a rate of 0.02987224 events/sec after 3 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.01098938, rate, 8, "the EWMA has a rate of 0.01098938 events/sec after 4 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.00404276, rate, 8, "the EWMA has a rate of 0.00404276 events/sec after 5 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.00148725, rate, 8, "the EWMA has a rate of 0.00148725 events/sec after 6 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.00054712, rate, 8, "the EWMA has a rate of 0.00054712 events/sec after 7 minutes");

            ElapseMinute(ewma);

            rate = ewma.Rate(TimeUnit.Seconds);
            AssertIsCloseTo(0.00020127, rate, 8, "the EWMA has a rate of 0.00020127 events/sec after 8 minutes");
        }

        private static void ElapseMinute(EWMA ewma)
        {
            for(var i = 1; i <= 12; i++)
            {
                ewma.Tick();
            }
        }

        public static void AssertIsCloseTo(double expected, double actual, int digits, string message)
        {
            var right = actual.ToString().Substring(0, digits + 2);
            var left = expected.ToString().Substring(0, digits + 2);
            Assert.AreEqual(Convert.ToDouble(left), Convert.ToDouble(right), message);
        }
    }
}
