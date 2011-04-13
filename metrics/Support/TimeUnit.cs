using System;

namespace metrics.Support
{
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

    public static class TimeUnitExtensions
    {
        internal static double ToNanos(this TimeUnit unit, long interval)
        {
            throw new NotImplementedException();
        }
    }
}
