using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using metrics.Core;
using metrics.Reporting;
using metrics.Support;

namespace metrics
{
    /// <summary>
    /// A set of factory methods for creating centrally registered metric instances
    /// </summary>
    /// <see href="https://github.com/codahale/metrics"/>
    /// <seealso href="http://codahale.com/codeconf-2011-04-09-metrics-metrics-everywhere.pdf" />
    public class Metrics
    {
        private static readonly ConcurrentDictionary<MetricName, IMetric> _metrics = new ConcurrentDictionary<MetricName, IMetric>();

        /// <summary>
        /// Creates a new counter gauge
        /// </summary>
        /// <typeparam name="T">The type the gauge measures</typeparam>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The gauge evaluation function</param>
        /// <returns></returns>
        public static GaugeMetric<T> Gauge<T>(Type type, string name, Func<T> evaluator)
        {
            return GetOrAdd(new MetricName(type, name), new GaugeMetric<T>(evaluator));
        }

        /// <summary>
        /// Enables the console reporter and causes it to print to STDOUT with the specified period
        /// </summary>
        /// <param name="period">The period between successive outputs</param>
        /// <param name="unit">The time unit of the period</param>
        public static void EnableConsoleReporting(long period, TimeUnit unit)
        {
            var reporter = new ConsoleReporter(Console.Out);
            reporter.Start(period, unit);
        }

        /// <summary>
        /// Creates a new counter metric
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public static CounterMetric Counter(Type type, string name)
        {
            return GetOrAdd(new MetricName(type, name), new CounterMetric());
        }
        
        /// <summary>
        /// Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public static IDictionary<MetricName, IMetric> All
        {
            get
            {
                return new ReadOnlyDictionary<MetricName, IMetric>(_metrics);   
            }
        }

        /// <summary>
        /// Clears all previously registered metrics
        /// </summary>
        public static void Clear()
        {
            _metrics.Clear();
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
    }
}
