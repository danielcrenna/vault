using NUnit.Framework;

namespace metrics.Tests
{
    [TestFixture]
    public class MachineMetricsTests
    {
        [Test]
        public void Can_load_all_metrics()
        {
            var metrics = new Metrics();
 
            metrics.Clear();
            var machineMetrics = new MachineMetrics(metrics);

            machineMetrics.Install();

            Assert.IsTrue(metrics.All.Count > 0);
        }
    }
}
