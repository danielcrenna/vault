using NUnit.Framework;

namespace metrics.Tests.Core
{
    public class MetricTestBase
    {
        [TearDown]
        public void TearDown()
        {
            var metrics = new Metrics();
 
            metrics.Clear();
        }
    }
}