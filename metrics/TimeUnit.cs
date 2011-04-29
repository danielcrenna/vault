using System;
using System.Diagnostics;

namespace metrics
{
    /// <summary>
    /// Provides support for timing values
    /// </summary>
    public enum TimeUnit
    {
        Nanoseconds = 0,
        Microseconds = 1,
        Milliseconds = 2,
        Seconds = 3,
        Minutes = 4,
        Hours = 5,
        Days = 6
    }

    /// <summary>
    /// Provides enum methods for timing values
    /// </summary>
    public static class TimeUnitExtensions
    {
        private static readonly long[] _timings = new[]
                                                     {
                                                         Stopwatch.Frequency/(1000L),
                                                         Stopwatch.Frequency/(1000L*1000L),
                                                         Stopwatch.Frequency/(1000L*1000L*1000L)
                                                     };

        public static long ToNanos(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Nanoseconds);
        }

        public static long ToMicros(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Microseconds);
        }

        public static long ToMillis(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Milliseconds);
        }

        public static long Convert(this TimeUnit source, long duration, TimeUnit target)
        {
           switch(source)
           {
               case TimeUnit.Nanoseconds:
                   switch(target)
                   {
                       case TimeUnit.Nanoseconds:
                           return duration;
                       case TimeUnit.Microseconds:
                           return duration / _timings[0];
                       case TimeUnit.Milliseconds:
                           return duration / _timings[1];
                       case TimeUnit.Seconds:
                           return duration / _timings[2];
                       default:
                           throw new ArgumentOutOfRangeException("target");
                   }
               case TimeUnit.Microseconds:
                   switch(target)
                   {
                       case TimeUnit.Nanoseconds:
                           return duration * _timings[0];
                       case TimeUnit.Microseconds:
                           return duration;
                       case TimeUnit.Milliseconds:
                           return duration / _timings[0];
                       case TimeUnit.Seconds:
                           return duration / _timings[1];
                       default:
                           throw new ArgumentOutOfRangeException("target");
                   }
               case TimeUnit.Milliseconds:
                   switch(target)
                   {
                       case TimeUnit.Nanoseconds:
                           return duration * _timings[1];
                       case TimeUnit.Microseconds:
                           return duration * _timings[0];
                       case TimeUnit.Milliseconds:
                           return duration;
                       case TimeUnit.Seconds:
                           return duration / _timings[0];
                       default:
                           throw new ArgumentOutOfRangeException("target");
                   }
               case TimeUnit.Seconds:
                   switch(target)
                   {
                       case TimeUnit.Nanoseconds:
                           return duration * _timings[2];
                       case TimeUnit.Microseconds:
                           return duration * _timings[1];
                       case TimeUnit.Milliseconds:
                           return duration * _timings[0];
                       case TimeUnit.Seconds:
                           return duration;
                       default:
                           throw new ArgumentOutOfRangeException("target");
                   }
               default:
                   throw new ArgumentOutOfRangeException("source");
           }
        }
    }
}