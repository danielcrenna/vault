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
    public class Metrics : IDisposable
    {
        private readonly ConcurrentDictionary<MetricName, IMetric> _metrics = new ConcurrentDictionary<MetricName, IMetric>();

        /// <summary>
        /// A convenience method for installing a gauge that is bound to a <see cref="PerformanceCounter" />
        /// </summary>
        /// <param name="category">The performance counter category</param>
        /// <param name="counter">The performance counter name</param>
        /// <param name="instance">The performance counter instance, if applicable</param>
        /// <param name="label">A label to distinguish the metric in polling reports</param>
        public  void InstallPerformanceCounterGauge(string category, string counter, string instance, string label)
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
        public  void InstallPerformanceCounterGauge(string category, string counter, string label)
        {
            var performanceCounter = new PerformanceCounter(category, counter, true);
            GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + label), new GaugeMetric<double>(() => performanceCounter.NextValue()));
        }

        /// <summary>
        /// Creates a new gauge metric and registers it under the given type and name
        /// </summary>
        /// <typeparam name="T">The type the gauge measures</typeparam>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The gauge evaluation function</param>
        /// <returns></returns>
        public  GaugeMetric<T> Gauge<T>(Type owner, string name, Func<T> evaluator)
        {
            return GetOrAdd(new MetricName(owner, name), new GaugeMetric<T>(evaluator));
        }

        /// <summary>
        /// Creates a new gauge metric and registers it under the given type and name
        /// </summary>
        /// <typeparam name="T">The type the gauge measures</typeparam>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The gauge evaluation function</param>
        /// <returns></returns>
        public GaugeMetric<T> Gauge<T>(string context, string name, Func<T> evaluator)
        {
            return GetOrAdd(new MetricName(context, name), new GaugeMetric<T>(evaluator));
        }

        /// <summary>
        /// Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public CounterMetric Counter(Type owner, string name)
        {
            return GetOrAdd(new MetricName(owner, name), new CounterMetric());
        }

        /// <summary>
        /// Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public CounterMetric Counter(string context, string name)
        {
            return GetOrAdd(new MetricName(context, name), new CounterMetric());
        }

        /// <summary>
        /// Creates a new histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="biased">Whether the sample type is biased or uniform</param>
        /// <returns></returns>
        public  HistogramMetric Histogram(Type owner, string name, bool biased)
        {
            return GetOrAdd(new MetricName(owner, name),
                            new HistogramMetric(biased
                                                    ? HistogramMetric.SampleType.Biased
                                                    : HistogramMetric.SampleType.Uniform));
        }
        /// <summary>
        /// Creates a new histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="biased">Whether the sample type is biased or uniform</param>
        /// <returns></returns>
        public HistogramMetric Histogram(string context, string name, bool biased)
        {
            return GetOrAdd(new MetricName(context, name),
                            new HistogramMetric(biased
                                                    ? HistogramMetric.SampleType.Biased
                                                    : HistogramMetric.SampleType.Uniform));
        }

        /// <summary>
        /// Creates a new non-biased histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public  HistogramMetric Histogram(Type owner, string name)
        {
            return GetOrAdd(new MetricName(owner, name), new HistogramMetric(HistogramMetric.SampleType.Uniform));
        }
        /// <summary>
        /// Creates a new non-biased histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public HistogramMetric Histogram(string context, string name)
        {
            return GetOrAdd(new MetricName(context, name), new HistogramMetric(HistogramMetric.SampleType.Uniform));
        }

        /// <summary>
        /// Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="unit">The rate unit of the new meter</param>
        /// <returns></returns>
        public  MeterMetric Meter(Type owner, string name, string eventType, TimeUnit unit)
        {
            var metricName = new MetricName(owner, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (MeterMetric) existingMetric;
            }

            var metric = new MeterMetric(eventType, unit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (MeterMetric) justAddedMetric;
        }

        /// <summary>
        /// Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="unit">The rate unit of the new meter</param>
        /// <returns></returns>
        public MeterMetric Meter(string context, string name, string eventType, TimeUnit unit)
        {
            var metricName = new MetricName(context, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (MeterMetric)existingMetric;
            }

            var metric = MeterMetric.New(eventType, unit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (MeterMetric)justAddedMetric;
        }
        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public  TimerMetric Timer(Type owner, String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
           var metricName = new MetricName(owner, name);
           IMetric existingMetric;
           if (_metrics.TryGetValue(metricName, out existingMetric))
           {
              return (TimerMetric)existingMetric;
           }

           var metric = new TimerMetric(durationUnit, rateUnit);
           var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
           return justAddedMetric == null ? metric : (TimerMetric)justAddedMetric;
        }

        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public TimerMetric Timer(string context, String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(context, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (TimerMetric)existingMetric;
            }

            var metric = new TimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (TimerMetric)justAddedMetric;
        }


        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="owner">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public  CallbackTimerMetric CallbackTimer(Type owner, String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
           var metricName = new MetricName(owner, name);
           IMetric existingMetric;
           if (_metrics.TryGetValue(metricName, out existingMetric))
           {
              return (CallbackTimerMetric)existingMetric;
           }

           var metric = new CallbackTimerMetric(durationUnit, rateUnit);
           var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
           return justAddedMetric == null ? metric : (CallbackTimerMetric)justAddedMetric;
        }

        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="context">The context for this metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public CallbackTimerMetric CallbackTimer(string context, String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(context, name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (CallbackTimerMetric)existingMetric;
            }

            var metric = new CallbackTimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (CallbackTimerMetric)justAddedMetric;
        }

       /// <summary>
       /// Creates a new metric that can be used to add manual timings into the system. A manual timing
       /// is a timing that is measured not by the metrics system but by the client site and must be added
       /// into metrics as an additional measurement.
       /// </summary>
       /// <param name="owner">The type that owns the metric</param>
       /// <param name="name">The metric name</param>
       /// <param name="durationUnit">The duration scale unit of the new timer</param>
       /// <param name="rateUnit">The rate unit of the new timer</param>
       /// <returns></returns>
       public  ManualTimerMetric ManualTimer(Type owner, String name, TimeUnit durationUnit, TimeUnit rateUnit)
       {
          var metricName = new MetricName(owner, name);
          IMetric existingMetric;
          if (_metrics.TryGetValue(metricName, out existingMetric))
          {
             return (ManualTimerMetric)existingMetric;
          }

          var metric = new ManualTimerMetric(durationUnit, rateUnit);
          var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
          return justAddedMetric == null ? metric : (ManualTimerMetric)justAddedMetric;
       }

       /// <summary>
       /// Creates a new metric that can be used to add manual timings into the system. A manual timing
       /// is a timing that is measured not by the metrics system but by the client site and must be added
       /// into metrics as an additional measurement.
       /// </summary>
       /// <param name="context">The context for this metric</param>
       /// <param name="name">The metric name</param>
       /// <param name="durationUnit">The duration scale unit of the new timer</param>
       /// <param name="rateUnit">The rate unit of the new timer</param>
       /// <returns></returns>
       public ManualTimerMetric ManualTimer(string context, String name, TimeUnit durationUnit, TimeUnit rateUnit)
       {
           var metricName = new MetricName(context, name);
           IMetric existingMetric;
           if (_metrics.TryGetValue(metricName, out existingMetric))
           {
               return (ManualTimerMetric)existingMetric;
           }

           var metric = new ManualTimerMetric(durationUnit, rateUnit);
           var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
           return justAddedMetric == null ? metric : (ManualTimerMetric)justAddedMetric;
       }

       /// <summary>
       /// Creates a new meter metric and registers it under the given type and name
       /// </summary>
       /// <param name="context">The context for this metric</param>
       /// <param name="name">The metric name</param>
       /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
       /// <param name="unit">The rate unit of the new meter</param>
       /// <param name="rate">The rate  of the new meter</param>
       /// <returns></returns>
       public PerSecondCounterMetric TimedCounter(string context, string name, string eventType)
       {
           var metricName = new MetricName(context, name);
           IMetric existingMetric;
           if (_metrics.TryGetValue(metricName, out existingMetric))
           {
               return (PerSecondCounterMetric)existingMetric;
           }

           var metric = PerSecondCounterMetric.New(eventType);
           var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
           return justAddedMetric == null ? metric : (PerSecondCounterMetric)justAddedMetric;
       }
        /// <summary>
        /// Enables the console reporter and causes it to print to STDOUT with the specified period
        /// </summary>
        /// <param name="period">The period between successive outputs</param>
        /// <param name="unit">The time unit of the period</param>
        public  void EnableConsoleReporting(long period, TimeUnit unit)
        {
            var reporter = new ConsoleReporter(this);
            EnableReporting(reporter, period, unit);
        }

        /// <summary>
        ///  Enables a reporter to run with the specified interval between outputs
        /// </summary>
        /// <param name="reporter"></param>
        /// <param name="period">The period between successive outputs</param>
        /// <param name="unit">The time unit of the period</param>
        public void EnableReporting(ReporterBase reporter, long period, TimeUnit unit)
        {
            reporter.Start(period, unit);
        }

        /// <summary>
        /// Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public  IDictionary<MetricName, IMetric> All
        {
            get { return new ReadOnlyDictionary<MetricName, IMetric>(_metrics); }
        }

        /// <summary>
        /// Returns a copy of all currently registered metrics in an immutable collection, sorted by owner and name
        /// </summary>
        public  IDictionary<MetricName, IMetric> AllSorted
        {
            get { return new ReadOnlyDictionary<MetricName, IMetric>(new SortedDictionary<MetricName, IMetric>(_metrics)); }
        }

        /// <summary>
        /// Clears all previously registered metrics
        /// </summary>
        public  void Clear()
        {
            _metrics.Clear();
            PerformanceCounter.CloseSharedResources();
        }

        private  T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
        {
            if (_metrics.ContainsKey(name))
            {
                return (T) _metrics[name];
            }

            var added = _metrics.AddOrUpdate(name, metric, (n, m) => m);

            return added == null ? metric : (T) added;
        }

        public void Dispose()
        {
            foreach (var metric in _metrics)
            {
                using (metric.Value as IDisposable)
                {
                    
                }
            }
        }
    }
}
