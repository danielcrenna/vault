using NUnit.Framework;

namespace metrics.Tests.Core
{
    public class MetricTestBase
    {
        [TearDown]
        public void TearDown()
        {
            Metrics.Clear();
        }
    }
}