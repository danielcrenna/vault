using System.Collections.Concurrent;

namespace ab
{
    public class MetricRepository
    {
        private static readonly ConcurrentDictionary<string, IMetric> Inner = new ConcurrentDictionary<string, IMetric>();
        public IMetric GetByName(string metric)
        {
            foreach (var item in Inner.Values)
            {
                if (item.Name == metric)
                {
                    return item;
                }
            }
            return null;
        }

        public void Save(IMetric metric)
        {
            Inner.AddOrUpdate(metric.Name, k => metric, (s, e) => metric);
        }
    }
}