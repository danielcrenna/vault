using System;
using metrics.Support;

namespace metrics.Core
{
    public class TimerMetric : IMetric, IMetered
    {
        public TimeUnit DurationUnit { get; set; }

        public IMetric Copy
        {
            get { throw new NotImplementedException(); }
        }

        public TimeUnit RateUnit
        {
            get { throw new NotImplementedException(); }
        }

        public string EventType
        {
            get { throw new NotImplementedException(); }
        }

        public long Count
        {
            get { throw new NotImplementedException(); }
        }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }

        public double FifteenMinuteRate()
        {
            throw new NotImplementedException();
        }

        public double FiveMinuteRate()
        {
            throw new NotImplementedException();
        }

        public double MeanRate()
        {
            throw new NotImplementedException();
        }

        public double OneMinuteRate()
        {
            throw new NotImplementedException();
        }

        public double[] Percentiles(params double[] args)
        {
            throw new NotImplementedException();
        }
    }
}