using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// A convenience method for installing a gauge that is bound to a <see cref="PerformanceCounter" />
        /// </summary>
        /// <param name="category">The performance counter category</param>
        /// <param name="counter">The performance counter name</param>
        /// <param name="instance">The performance counter instance, if applicable</param>
        /// <param name="label">A label to distinguish the metric in polling reports</param>
        public static void InstallPerformanceCounterGauge(string category, string counter, string instance, string label)
        {
            var performanceCounter = new PerformanceCounter(category, counter, instance, true);
            GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + label), new GaugeMetric<double>(() => performanceCounter.NextValue()));
        }

        /// <summary>
        /// A convenience method for installing a gauge that is bound to a <see cref="PerformanceCounter" />
        /// </summary>
        /// <param name="category">The performance counter category</param>
        /// <param name="counter">The performance counter name</param>
        /// <param name="label">A label to distinguish the metric in polling reports</param>
        public static void InstallPerformanceCounterGauge(string category, string counter, string label)
        {
            var performanceCounter = new PerformanceCounter(category, counter, true);
            GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + label), new GaugeMetric<double>(() => performanceCounter.NextValue()));
        }

        /// <summary>
        /// Creates a new gauge metric and registers it under the given type and name
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
        /// Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public static CounterMetric Counter(Type type, string name)
        {
            return GetOrAdd(new MetricName(type, name), new CounterMetric());
        }

        /// <summary>
        /// Creates a new histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="biased">Whether the sample type is biased or uniform</param>
        /// <returns></returns>
        public static HistogramMetric Histogram(Type type, string name, bool biased)
        {
            return GetOrAdd(new MetricName(type, name),
                            new HistogramMetric(biased
                                                    ? HistogramMetric.SampleType.Biased
                                                    : HistogramMetric.SampleType.Uniform));
        }

        /// <summary>
        /// Creates a new non-biased histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public static HistogramMetric Histogram(Type type, string name)
        {
            return GetOrAdd(new MetricName(type, name), new HistogramMetric(HistogramMetric.SampleType.Uniform));
        }

        /// <summary>
        /// Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="unit">The rate unit of the new meter</param>
        /// <returns></returns>
        public static MeterMetric Meter(Type type, string name, string eventType, TimeUnit unit)
        {
            var metricName = new MetricName(type, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (MeterMetric) existingMetric;
            }

            var metric = MeterMetric.New(eventType, unit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (MeterMetric) justAddedMetric;
        }

        /// <summary>
        /// reates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public static TimerMetric Timer(Type type, String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(type, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (TimerMetric) existingMetric;
            }

            var metric = new TimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (TimerMetric) justAddedMetric;
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
        /// Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public static IDictionary<MetricName, IMetric> AllSorted
        {
            get
            {
                return new ReadOnlyDictionary<MetricName, IMetric>(new SortedDictionary<MetricName, IMetric>(_metrics));
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
