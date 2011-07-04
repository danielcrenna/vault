using System;
using System.IO;
using System.Threading;
using metrics.Core;
using metrics.Util;

namespace metrics.Reporting
{
    /// <summary>
    ///  A simple reporters which prints out application metrics to a <see cref="TextWriter" /> periodically
    /// </summary>
    public class ConsoleReporter
    {
        private static readonly NamedThreadFactory _factory = new NamedThreadFactory("metrics-console-reporter");
        private Thread _tickThread;
        private readonly TextWriter _out;

        public ConsoleReporter(TextWriter @out)
        {
            _out = @out;
        }

        /// <summary>
        /// Starts printing output to the specified <see cref="TextWriter" />
        /// </summary>
        /// <param name="period">The period between successive displays</param>
        /// <param name="unit">The period time unit</param>
        public void Start(long period, TimeUnit unit)
        {
            var seconds = unit.Convert(period, TimeUnit.Seconds);
            var interval = TimeSpan.FromSeconds(seconds);

            if(_tickThread != null)
            {
                _tickThread.Abort();
            }

            _tickThread = _factory.New(
                () =>
                    {
                        new Timer(s => Run(), null, interval, interval);
                    }
                );
            _tickThread.Start();
        }

        public void Run()
        {
            try
            {
                var now = DateTime.Now;
                var dateTime = now.ToShortDateString() + " " + now.ToShortTimeString();
                _out.Write(dateTime);
                _out.Write(' ');
                for (var i = 0; i < (80 - dateTime.Length - 1); i++)
                {
                    _out.Write('=');
                }
                _out.WriteLine();

                foreach (var entry in Utils.SortMetrics(Metrics.All))
                {
                    _out.Write(entry.Key);
                    _out.WriteLine(':');

                    foreach (var subEntry in entry.Value)
                    {
                        _out.Write("  ");
                        _out.Write(subEntry.Key);
                        _out.WriteLine(':');

                        var metric = subEntry.Value;
                        if (metric is GaugeMetric)
                        {
                            WriteGauge((GaugeMetric) metric);
                        }
                        else if (metric is CounterMetric)
                        {
                            WriteCounter((CounterMetric) metric);
                        }
                        else if (metric is HistogramMetric)
                        {
                            WriteHistogram((HistogramMetric) metric);
                        }
                        else if (metric is MeterMetric)
                        {
                            WriteMetered((MeterMetric) metric);
                        }
                        else if (metric is TimerMetric)
                        {
                            WriteTimer((TimerMetric) metric);
                        }
                        _out.WriteLine();
                    }
                    _out.WriteLine();
                    _out.Flush();
                }
            }
            catch (Exception e)
            {
                _out.WriteLine(e.StackTrace);
            }
        }

        private void WriteGauge(GaugeMetric gauge)
        {
            _out.Write("    value = ");
            _out.WriteLine(gauge.ValueAsString);
        }

        private void WriteCounter(CounterMetric counter)
        {
            _out.Write("    count = ");
            _out.WriteLine(counter.Count);
        }

        private void WriteMetered(IMetered meter)
        {
            var unit = Abbreviate(meter.RateUnit);
            _out.Write("             count = {0}\n", meter.Count);
            _out.Write("         mean rate = {0} {1}/{2}\n", meter.MeanRate, meter.EventType, unit);
            _out.Write("     1-minute rate = {0} {1}/{2}\n", meter.OneMinuteRate, meter.EventType, unit);
            _out.Write("     5-minute rate = {0} {1}/{2}\n", meter.FiveMinuteRate, meter.EventType, unit);
            _out.Write("    15-minute rate = {0} {1}/{2}\n", meter.FifteenMinuteRate, meter.EventType, unit);
        }

        private void WriteHistogram(HistogramMetric histogram)
        {
            var percentiles = histogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

            _out.Write("               min = %{0:2}\n", histogram.Min);
            _out.Write("               max = %{0:2}\n", histogram.Max);
            _out.Write("              mean = %{0:2}\n", histogram.Mean);
            _out.Write("            stddev = %{0:2}\n", histogram.StdDev);
            _out.Write("            median = %{0:2}\n", percentiles[0]);
            _out.Write("              75%% <= %{0:2}\n", percentiles[1]);
            _out.Write("              95%% <= %{0:2}\n", percentiles[2]);
            _out.Write("              98%% <= %{0:2}\n", percentiles[3]);
            _out.Write("              99%% <= %{0:2}\n", percentiles[4]);
            _out.Write("            99.9%% <= %{0:2}\n", percentiles[5]);
        }
       
        private void WriteTimer(TimerMetric timer)
        {
            WriteMetered(timer);

            var durationUnit = Abbreviate(timer.DurationUnit);

            var percentiles = timer.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

            _out.Write("               min = %{0:2}{1}\n", timer.Min, durationUnit);
            _out.Write("               max = %{0:2}{1}\n", timer.Max, durationUnit);
            _out.Write("              mean = %{0:2}{1}\n", timer.Mean, durationUnit);
            _out.Write("            stddev = %{0:2}{1}\n", timer.StdDev, durationUnit);
            _out.Write("            median = %{0:2}{1}\n", percentiles[0], durationUnit);
            _out.Write("              75%% <= %{0:2}{1}\n", percentiles[1], durationUnit);
            _out.Write("              95%% <= %{0:2}{1}\n", percentiles[2], durationUnit);
            _out.Write("              98%% <= %{0:2}{1}\n", percentiles[3], durationUnit);
            _out.Write("              99%% <= %{0:2}{1}\n", percentiles[4], durationUnit);
            _out.Write("            99.9%% <= %{0:2}{1}\n", percentiles[5], durationUnit);
        }

        private static string Abbreviate(TimeUnit unit)
        {
            switch(unit)
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
