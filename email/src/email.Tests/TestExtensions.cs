using System;

namespace email.Tests
{
    public static class TestExtensions
    {
        public static TimeSpan Seconds(this int input)
        {
            return TimeSpan.FromSeconds(input);
        }
        public static DateTime FromNow(this TimeSpan ts)
        {
            return DateTime.UtcNow.Add(ts);
        }
    }
}