using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using metrics.Core;

namespace metrics.Util
{
    internal static class Utils
    {
        internal static IDictionary<string, IDictionary<string, IMetric>> SortMetrics(IDictionary<MetricName, IMetric> metrics)
        {
            var sortedMetrics = new SortedDictionary<string, IDictionary<string, IMetric>>();

            foreach(var entry in metrics)
            {
                var className = entry.Key.Class.Name;
                IDictionary<string, IMetric> submetrics;
                if(!sortedMetrics.ContainsKey(className))
                {
                    submetrics = new SortedDictionary<string, IMetric>();
                    sortedMetrics.Add(className, submetrics);
                }
                else
                {
                    submetrics = sortedMetrics[className];
                }
                submetrics.Add(entry.Key.Name, entry.Value);
            }
            return sortedMetrics;
        }

        internal static CancellationTokenSource StartCancellableTask(Action closure)
        {
            var source = new CancellationTokenSource();
            new TaskFactory().StartNew(closure, source.Token);
            return source;
        }
    }
}
