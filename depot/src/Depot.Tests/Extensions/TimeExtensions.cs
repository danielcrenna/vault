using System;

namespace depot.Tests.Extensions
{
    internal static class TimeExtensions
    {
        public static double Elapsed(this DateTime dateTime)
        {
            return (DateTime.Now.Ticks - dateTime.Ticks).Elapsed();
        }

        public static double Elapsed(this long ticks)
        {
            return (DateTime.Now.Ticks - ticks).Ticks().TotalSeconds;
        }

        public static TimeSpan Ticks(this long value)
        {
            return TimeSpan.FromTicks(value);
        }

        public static TimeSpan Days(this int value)
        {
            return TimeSpan.FromDays(value);
        }

        public static TimeSpan Months(this int value)
        {
            return TimeSpan.FromDays(value * 30);
        }

        public static TimeSpan Weeks(this int value)
        {
            return TimeSpan.FromDays(value * 7);
        }

        public static DateTime FromNow(this TimeSpan value)
        {
            return DateTime.UtcNow.Add(value);
        }

        public static TimeSpan Hours(this int value)
        {
            return TimeSpan.FromHours(value);
        }

        public static TimeSpan Minutes(this int value)
        {
            return TimeSpan.FromMinutes(value);
        }

        public static TimeSpan Seconds(this int value)
        {
            return TimeSpan.FromSeconds(value);
        }

        public static TimeSpan Milliseconds(this int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }

        public static TimeSpan Passed(this DateTime time)
        {
            return DateTime.Now.Subtract(time).Duration();
        }
    }
}