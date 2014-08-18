using System.Linq;
using NUnit.Framework;
using metrics.Core;
using metrics.Stats;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class HistogramMetricTests
    {
        [Test]
        public void Max_WhenPassed8And9_Returns9()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(9, underTest.Max);
        }

        [Test]
        public void Min_WhenPassed8And9_Returns9()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8, underTest.Min);
        }

        [Test]
        public void Count_WhenPassedTwoValues_Returns2()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(2, underTest.Count);
        }

        [Test]
        public void Mean_WhenPassed8And9_Returns8Point5()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8.5, underTest.Mean);
        }

        [Test]
        public void SampleMax_WhenThereAreNoValues_Returns0()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            Assert.AreEqual(0, underTest.SampleMax);
        }

        [Test]
        public void SampleMax_UsesSample()
        {
            var underTest = new HistogramMetric(new RejectEverythingAboveTenSample());
            underTest.Update(11);
            underTest.Update(8);
            Assert.AreEqual(8, underTest.SampleMax);
        }

        [Test]
        public void SampleMax_WhenPassed8And9_Returns9()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(9, underTest.SampleMax);
        }

        [Test]
        public void SampleMin_WhenPassed8And9_Returns8()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8, underTest.SampleMin);
        }

        [Test]
        public void SampleMin_WhenThereAReNoValues_Returns0()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            Assert.AreEqual(0, underTest.SampleMin);
        }

        [Test]
        public void SampleMin_UsesSample()
        {
            var underTest = new HistogramMetric(new RejectEverythingAboveTenSample());
            underTest.Update(11);
            underTest.Update(12);
            Assert.AreEqual(0, underTest.SampleMin);
        }

        [Test]
        public void SampleCount_WhenPassed8And9_Returns2()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(2, underTest.SampleCount);
        }

        [Test]
        public void SampleCount_UsesSample()
        {
            var underTest = new HistogramMetric(new RejectEverythingAboveTenSample());
            underTest.Update(11);
            underTest.Update(2);
            Assert.AreEqual(2, underTest.SampleMin);
        }

        [Test]
        public void SampleCount_WhenThereAreNoValues_Returns0()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            Assert.AreEqual(0, underTest.SampleCount);
        }

        [Test]
        public void SampleMean_WhenPassed8And9_Returns8Point5()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8.5, underTest.SampleMean);
        }

        [Test]
        public void SampleMean_UsesSample()
        {
            var underTest = new HistogramMetric(new RejectEverythingAboveTenSample());
            underTest.Update(5);
            underTest.Update(12);
            Assert.AreEqual(5, underTest.SampleMin);
        }

        [Test]
        public void SampleMean_WhenThereAreNoValues_Returns0()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Uniform);
            Assert.AreEqual(0, underTest.SampleMean);
        }

        [Test]
        public void BiasedHistogram_UsesSample()
        {
            var underTest = new HistogramMetric(HistogramMetric.SampleType.Biased);
            var sampleSize = 1028;
            var sample = Enumerable.Range(1, sampleSize).ToList();

            foreach (var s in sample)
            {
                underTest.Update(s);
            }

            Assert.AreEqual(sampleSize, underTest.SampleCount);
            CollectionAssert.AreEquivalent(sample, underTest.Values);
        }

        private class RejectEverythingAboveTenSample : UniformSample
        {
            public RejectEverythingAboveTenSample()
                : base(10)
            { }

            public override void Update(long value)
            {
                if (value <= 10)
                    base.Update(value);
            }
        }
    }
}
