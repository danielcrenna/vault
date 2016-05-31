using NUnit.Framework;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class HealthChecksTests
    {
        [Test]
        public void Correctly_Report_When_There_Are_HealthChecks()
        {
            HealthChecks.Register("test-health-check", () => HealthCheck.Result.Healthy);
            Assert.That(HealthChecks.HasHealthChecks, Is.True);
        }
    }
}
