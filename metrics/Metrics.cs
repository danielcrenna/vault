using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using metrics.Core;
using metrics.Support;

namespace metrics
{
    public class Metrics
    {
        private static readonly ConcurrentDictionary<MetricName, IMetric> _metrics = new ConcurrentDictionary<MetricName, IMetric>();

        public static CounterMetric Counter(Type type, string name)
        {
            return GetOrAdd(new MetricName(type, name), new CounterMetric());
        }
        
        public static IDictionary<MetricName, IMetric> All
        {
            get
            {
                return new ReadOnlyDictionary<MetricName, IMetric>(_metrics);   
            }
        }

        private static T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
        {
            if(_metrics.ContainsKey(name))
            {
                return (T) _metrics[name];
            }

            var added = _metrics.AddOrUpdate(name, metric, (n, m) => m);

            return added == null ? metric : (T) added;
        }

        public static void Clear()
        {
            _metrics.Clear();
        }
    }
}
