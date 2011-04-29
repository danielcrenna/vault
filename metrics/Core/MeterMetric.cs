using System;
using metrics.Support;

namespace metrics.Core
{
    /// <summary>
    /// A meter metric which measures mean throughput and one-, five-, and fifteen-minute exponentially-weighted moving average throughputs
    /// </summary>
    /// <see href="http://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average">EMA</see>
    public class MeterMetric : IMetered 
    {
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

        public static MeterMetric New(string calls, TimeUnit rateUnit)
        {
            throw new NotImplementedException();
        }

        public void Mark()
        {
            throw new NotImplementedException();
        }
    }
}
