using metrics.Serialization;

namespace metrics.Reporting
{
    public class JsonReportFormatter : IReportFormatter
    {
        public string GetSample()
        {
            return Serializer.Serialize(Metrics.AllSorted);
        }
    }
}