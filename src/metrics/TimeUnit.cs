using System;

namespace metrics
{
    /// <summary>
    /// Provides support for timing values
    /// <see href="http://download.oracle.com/javase/6/docs/api/java/util/concurrent/TimeUnit.html"/>
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
		private static readonly long[][] _conversionMatrix = BuildConversionMatrix();

		private static long[][] BuildConversionMatrix()
		{
			var unitsCount = Enum.GetValues(typeof(TimeUnit)).Length;
			var timingFactors = new[] 
			{
				1000L, // nanos to micros
				1000L, // micros to millis
				1000L, // millis to seconds
				60L, // seconds to minutes
				60L, // minutes to hours
				24L // hours to days
			};

			// matrix[i, j] holds the timing factor we need to divide by to get from i to j.
			// we'll only populate the part of the matrix where j < i since the other half uses the same factors.
			var matrix = new long[unitsCount][];
			for (var source = 0; source < unitsCount; source++)
			{
				matrix[source] = new long[source];
				var cumulativeFactor = 1L;
				for (var target = source - 1; target >= 0; target--)
				{
					cumulativeFactor *= timingFactors[target];
					matrix[source][target] = cumulativeFactor;
				}
			}

			return matrix;
		}

		public static long Convert(this TimeUnit source, long duration, TimeUnit target)
		{
			if (source == target) return duration;

			var sourceIndex = (int)source;
			var targetIndex = (int)target;

			return (sourceIndex > targetIndex) ?
				duration * _conversionMatrix[sourceIndex][targetIndex] :
				duration / _conversionMatrix[targetIndex][sourceIndex];
		}

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

        public static long ToSeconds(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Seconds);
        }

        public static long ToMinutes(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Minutes);
        }

        public static long ToHours(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Hours);
        }

        public static long ToDays(this TimeUnit source, long interval)
        {
            return Convert(source, interval, TimeUnit.Days);
        }
    }
}