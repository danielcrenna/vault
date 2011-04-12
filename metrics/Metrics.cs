using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using metrics.Core;
using metrics.Support;

namespace metrics
{
    /// <summary>
    /// 
    /// </summary>
    /// <see href="https://github.com/codahale/metrics"/>
    /// <seealso href="http://codahale.com/codeconf-2011-04-09-metrics-metrics-everywhere.pdf" />
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
