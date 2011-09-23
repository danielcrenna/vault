using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace metrics.Core
{
    /// <summary>
    /// A timer metric which aggregates timing durations and provides duration
    /// statistics, plus throughput statistics via <see cref="MeterMetric" />.
    /// </summary>
    public class TimerMetric : IMetric, IMetered
    {
        private readonly MeterMetric _meter;
        private readonly HistogramMetric _histogram;

        public TimerMetric(TimeUnit durationUnit, TimeUnit rateUnit)
            : this(durationUnit, rateUnit, MeterMetric.New("calls", rateUnit), new HistogramMetric(HistogramMetric.SampleType.Biased), true /* clear */)
        {

        }

        private TimerMetric(TimeUnit durationUnit, TimeUnit rateUnit, MeterMetric meter, HistogramMetric histogram, bool clear)
        {
            DurationUnit = durationUnit;
            RateUnit = rateUnit;
            _meter = meter;
            _histogram = histogram;
            if(clear)
            {
                Clear();
            }
        }

        /// <summary>
        ///  Returns the timer's duration scale unit
        /// </summary>
        public TimeUnit DurationUnit { get; private set; }

        /// <summary>
        /// Returns the meter's rate unit
        /// </summary>
        /// <returns></returns>
        public TimeUnit RateUnit { get; private set; }

        /// <summary>
        /// Clears all recorded durations
        /// </summary>
        public void Clear()
        {
            _histogram.Clear();
        }

        public void Update(long duration, TimeUnit unit)
        {
            Update(unit.ToNanos(duration));
        }

        /// <summary>
        /// Times and records the duration of an event
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the event</typeparam>
        /// <param name="event">A function whose duration should be timed</param>
        public T Time<T>(Func<T> @event)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                return @event.Invoke();
            }
            finally
            {
                stopwatch.Stop();
                Update(stopwatch.ElapsedTicks * (1000L * 1000L * 1000L) / Stopwatch.Frequency);
            }
        }

        /// <summary>
        /// Times and records the duration of an event
        /// </summary>
        /// <param name="event">An action whose duration should be timed</param>
        public void Time(Action @event)
        {
            Time(() =>
            {
                @event.Invoke();
                return null as object;
            });
        }

        /// <summary>
        ///  Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        public long Count
        {
            get { return _histogram.Count; }
        }

        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        public double FifteenMinuteRate
        {
            get { return _meter.FifteenMinuteRate; }
        }

        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        public double FiveMinuteRate
        {
            get { return _meter.FiveMinuteRate; }
        }

        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        public double MeanRate
        {
            get { return _meter.MeanRate; }
        }

        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public double OneMinuteRate
        {
            get { return _meter.OneMinuteRate; }
        }

        /// <summary>
        /// Returns the longest recorded duration
        /// </summary>
        public double Max
        {
            get { return ConvertFromNanos(_histogram.Max); }
        }

        /// <summary>
        /// Returns the shortest recorded duration
        /// </summary>
        public double Min
        {
            get { return ConvertFromNanos(_histogram.Min); }
        }

        /// <summary>
        ///  Returns the arithmetic mean of all recorded durations
        /// </summary>
        public double Mean
        {
            get { return ConvertFromNanos(_histogram.Mean); }
        }

        /// <summary>
        /// Returns the standard deviation of all recorded durations
        /// </summary>
        public double StdDev
        {
            get { return ConvertFromNanos(_histogram.StdDev); }
        }

        /// <summary>
        /// Returns an array of durations at the given percentiles
        /// </summary>
        public double[] Percentiles(params double[] percentiles)
        {
            var scores = _histogram.Percentiles(percentiles);
            for (var i = 0; i < scores.Length; i++)
            {
                scores[i] = ConvertFromNanos(scores[i]);
            }

            return scores;
        }

        /// <summary>
        /// Returns the type of events the meter is measuring
        /// </summary>
        /// <returns></returns>
        public string EventType
        {
            get { return _meter.EventType; }
        }

        /// <summary>
        /// Returns a list of all recorded durations in the timers's sample
        /// </summary>
        public ICollection<double> Values
        {
            get
            {
                return _histogram.Values.Select(value => ConvertFromNanos(value)).ToList();
            }
        }
		
        private void Update(long duration)
        {
            if (duration < 0) return;
            _histogram.Update(duration);
            _meter.Mark();
        }

        private double ConvertFromNanos(double nanos)
        {
            return nanos / DurationUnit.Convert(1, TimeUnit.Nanoseconds);
        }
        
        [JsonIgnore]
        public IMetric Copy
        {
            get
            {
                var copy = new TimerMetric(
                    DurationUnit, RateUnit, _meter, _histogram, false /* clear */
                    );
                return copy;
            }
        }
    }
}