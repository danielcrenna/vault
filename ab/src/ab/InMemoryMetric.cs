using System;
using metrics;

namespace ab
{
    public sealed class InMemoryMetric : MetricBase
    {
        internal const string Separator = "__";
        internal const string Header = "__m__track__";

        public InMemoryMetric(string name)
        {
            Name = name;
        }
        
        public override double[] Values(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public override void OnHook(MetricEventArgs e)
        {
            var counter = Metrics.Counter(typeof(M), InternalMetric(e.Name, e.Timestamp));
            counter.Increment(e.Value);
            base.OnHook(e);
        }
        
        private static string InternalMetric(string tag, long timestamp)
        {
            return string.Concat(Header, tag, Separator, timestamp);
        }
    }
}