using System;
using System.Threading;
using metrics.Support;

namespace metrics.Stats
{
    /// <summary>
    ///  An exponentially-weighted moving average
    /// </summary>
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg1.pdf"/>
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg2.pdf" />
    public class EWMA
    {
        private static readonly double M1Alpha = 1 - Math.Exp(-5 / 60.0);
        private static readonly double M5Alpha = 1 - Math.Exp(-5 / 60.0 / 5);
        private static readonly double M15Alpha = 1 - Math.Exp(-5 / 60.0 / 15);

        private long _uncounted;
        private readonly double _alpha;
        private readonly double _interval;
        private volatile bool _initialized;
        private /* volatile */ double _rate;

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX one minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        public static EWMA OneMinuteEWMA()
        {
            return new EWMA(M1Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX five minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FiveMinuteEWMA()
        {
            return new EWMA(M5Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        ///  Creates a new EWMA which is equivalent to the UNIX fifteen minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FifteenMinuteEWMA()
        {
            return new EWMA(M15Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        /// Create a new EWMA with a specific smoothing constant.
        /// </summary>
        /// <param name="alpha">The smoothing constant</param>
        /// <param name="interval">The expected tick interval</param>
        /// <param name="intervalUnit">The time unit of the tick interval</param>
        public EWMA(double alpha, long interval, TimeUnit intervalUnit)
        {
            _interval = intervalUnit.ToNanos(interval);
            _alpha = alpha;
        }

       /// <summary>
        ///  Update the moving average with a new value.
       /// </summary>
       /// <param name="n"></param>
        public void Update(long n)
        {
            Interlocked.Add(ref _uncounted, n);
        }

        /// <summary>
        /// Mark the passage of time and decay the current rate accordingly.
        /// </summary>
        public void Tick()
        {
            var count = Interlocked.Read(ref _uncounted);
            var instantRate = count / _interval;
            if (_initialized)
            {
                Thread.VolatileWrite(ref _rate, (_alpha * (instantRate - Thread.VolatileRead(ref _rate))));
            }
            else
            {
                Thread.VolatileWrite(ref _rate, instantRate);
                _initialized = true;
            }
        }

        /// <summary>
        /// Returns the rate in the given units of time.
        /// </summary>
        public double Rate(TimeUnit rateUnit)
        {
            return Thread.VolatileRead(ref _rate) * rateUnit.ToNanos(1);
        }
    }
}
