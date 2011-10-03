using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using metrics.Util;

namespace metrics.Core
{
    public abstract class ReporterBase : IReporter
    {
        protected TextWriter Out;
        protected CancellationTokenSource Token;
        internal int Runs { get; set; }

        protected ReporterBase(TextWriter writer)
        {
            Out = writer;
        }

        /// <summary>
        /// Starts printing output to the specified <see cref="TextWriter" />
        /// </summary>
        /// <param name="period">The period between successive displays</param>
        /// <param name="unit">The period time unit</param>
        public virtual void Start(long period, TimeUnit unit)
        {
            var seconds = unit.Convert(period, TimeUnit.Seconds);
            var interval = TimeSpan.FromSeconds(seconds);

            Token = Utils.StartCancellableTask(() =>
            {
                while (!Token.IsCancellationRequested)
                {
                    Thread.Sleep(interval);
                    Run();
                }
            });
        }

        public virtual void Run()
        {
            try
            {
                var now = DateTime.Now;
                var dateTime = string.Format("{0} {1}", now.ToShortDateString(), now.ToShortTimeString());
                Out.Write(dateTime);
                Out.Write(' ');
                for (var i = 0; i < (80 - dateTime.Length - 1); i++)
                {
                    Out.Write('=');
                }
                Out.WriteLine();

                foreach (var entry in Utils.SortMetrics(Metrics.All))
                {
                    Out.Write(entry.Key);
                    Out.WriteLine(':');

                    foreach (var subEntry in entry.Value)
                    {
                        Out.Write("  ");
                        Out.Write(subEntry.Key);
                        Out.WriteLine(':');

                        var metric = subEntry.Value;
                        if (metric is GaugeMetric)
                        {
                            WriteGauge((GaugeMetric)metric);
                        }
                        else if (metric is CounterMetric)
                        {
                            WriteCounter((CounterMetric)metric);
                        }
                        else if (metric is HistogramMetric)
                        {
                            WriteHistogram((HistogramMetric)metric);
                        }
                        else if (metric is MeterMetric)
                        {
                            WriteMetered((MeterMetric)metric);
                        }
                        else if (metric is TimerMetric)
                        {
                            WriteTimer((TimerMetric)metric);
                        }
                        Out.WriteLine();
                    }
                    Out.WriteLine();
                    Out.Flush();
                }

                Runs++;
            }
            catch (Exception e)
            {
                Out.WriteLine(e.StackTrace);
            }
        }

        public void Dispose()
        {
            if (Token != null)
                Token.Cancel();

            if (Out != null)
                Out.Close();
        }

        protected void WriteGauge(GaugeMetric gauge)
        {
            Out.Write("    value = ");
            Out.WriteLine(gauge.ValueAsString);
        }

        protected void WriteCounter(CounterMetric counter)
        {
            Out.Write("    count = ");
            Out.WriteLine(counter.Count);
        }

        protected void WriteMetered(IMetered meter)
        {
            var unit = Abbreviate(meter.RateUnit);
            Out.Write("             count = {0}\n", meter.Count);
            Out.Write("         mean rate = {0} {1}/{2}\n", meter.MeanRate, meter.EventType, unit);
            Out.Write("     1-minute rate = {0} {1}/{2}\n", meter.OneMinuteRate, meter.EventType, unit);
            Out.Write("     5-minute rate = {0} {1}/{2}\n", meter.FiveMinuteRate, meter.EventType, unit);
            Out.Write("    15-minute rate = {0} {1}/{2}\n", meter.FifteenMinuteRate, meter.EventType, unit);
        }

        protected void WriteHistogram(HistogramMetric histogram)
        {
            var percentiles = histogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

            Out.Write("               min = %{0:2}\n", histogram.Min);
            Out.Write("               max = %{0:2}\n", histogram.Max);
            Out.Write("              mean = %{0:2}\n", histogram.Mean);
            Out.Write("            stddev = %{0:2}\n", histogram.StdDev);
            Out.Write("            median = %{0:2}\n", percentiles[0]);
            Out.Write("              75%% <= %{0:2}\n", percentiles[1]);
            Out.Write("              95%% <= %{0:2}\n", percentiles[2]);
            Out.Write("              98%% <= %{0:2}\n", percentiles[3]);
            Out.Write("              99%% <= %{0:2}\n", percentiles[4]);
            Out.Write("            99.9%% <= %{0:2}\n", percentiles[5]);
        }

        protected void WriteTimer(TimerMetric timer)
        {
            WriteMetered(timer);

            var durationUnit = Abbreviate(timer.DurationUnit);

            var percentiles = timer.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

            Out.Write("               min = %{0:2}{1}\n", timer.Min, durationUnit);
            Out.Write("               max = %{0:2}{1}\n", timer.Max, durationUnit);
            Out.Write("              mean = %{0:2}{1}\n", timer.Mean, durationUnit);
            Out.Write("            stddev = %{0:2}{1}\n", timer.StdDev, durationUnit);
            Out.Write("            median = %{0:2}{1}\n", percentiles[0], durationUnit);
            Out.Write("              75%% <= %{0:2}{1}\n", percentiles[1], durationUnit);
            Out.Write("              95%% <= %{0:2}{1}\n", percentiles[2], durationUnit);
            Out.Write("              98%% <= %{0:2}{1}\n", percentiles[3], durationUnit);
            Out.Write("              99%% <= %{0:2}{1}\n", percentiles[4], durationUnit);
            Out.Write("            99.9%% <= %{0:2}{1}\n", percentiles[5], durationUnit);
        }

        protected static string Abbreviate(TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Nanoseconds:
                    return "ns";
                case TimeUnit.Microseconds:
                    return "us";
                case TimeUnit.Milliseconds:
                    return "ms";
                case TimeUnit.Seconds:
                    return "s";
                case TimeUnit.Minutes:
                    return "m";
                case TimeUnit.Hours:
                    return "h";
                case TimeUnit.Days:
                    return "d";
                default:
                    throw new ArgumentOutOfRangeException("unit");
            }
        }
    }
}
