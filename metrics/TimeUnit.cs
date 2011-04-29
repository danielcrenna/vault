using System.Diagnostics;

namespace metrics
{
    /// <summary>
    /// Provides support for timing values
    /// </summary>
    public enum TimeUnit
    {
        Nanoseconds,
        Microseconds,
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days
    }

    /// <summary>
    /// Provides enum methods for timing values
    /// </summary>
    public static class TimeUnitExtensions
    {
        internal static long ToNanos(this TimeUnit unit, long interval)
        {
            return interval / Stopwatch.Frequency / 1000L * 1000L * 1000L;
        }
        internal static long ToMicros(this TimeUnit unit, long interval)
        {
            return interval / Stopwatch.Frequency / 1000L * 1000L;
        }
        internal static long ToMillis(this TimeUnit unit, long interval)
        {
            return interval / Stopwatch.Frequency / 1000L;
        }
    }
}