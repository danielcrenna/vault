using metrics.Serialization;

namespace metrics.Reporting
{
    public class JsonReportFormatter : IReportFormatter
    {
        public string GetSample()
        {
            var metrics = new Metrics();
 
            return Serializer.Serialize(metrics.AllSorted);
        }
    }
}