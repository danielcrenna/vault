using System.Diagnostics;
using NUnit.Framework;
using Specifications.Extensions;

namespace Specifications.Tests
{
    [TestFixture]
    public class SpecificationTests
    {
        [Test]
        public void Can_satisfy_specifications()
        {
            ISpecification<bool> spec = new PredicateSpecification<bool>(v => v);
            Assert.AreEqual(true, spec.IsSatisfiedBy(true));
        }

        [Test]
        public void Can_chain_specifications()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isLessThanFive = new PredicateSpecification<int>(v => v < 5);
            var isFour = isGreaterThanThree.And(isLessThanFive);
            Assert.AreEqual(true, isFour.IsSatisfiedBy(2 + 2));
            Assert.AreEqual(true, (2 + 2).Satisfies(isFour));
        }

        [Test]
        public void Can_bench_specification_extension_method_performance()
        {
            var stopwatch = new Stopwatch();
            const int trials = 1000;

            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isLessThanFive = new PredicateSpecification<int>(v => v < 5);
            var isFour = isGreaterThanThree.And(isLessThanFive);

            stopwatch.Start();
            
            for (var i = 0; i < trials; i++)
            {
                isFour.IsSatisfiedBy(2 + 2);
            }
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.Elapsed);

            stopwatch.Start();
            for (var i = 0; i < trials; i++)
            {
                (2 + 2).Satisfies(isFour);
            }
            stopwatch.Stop();
            Trace.WriteLine(stopwatch.Elapsed);
        }
    }
}
