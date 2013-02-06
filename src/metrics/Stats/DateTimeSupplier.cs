using System;

namespace metrics.Stats
{
    internal class DateTimeSupplier : IDateTimeSupplier
    {
        public DateTime Now { get { return DateTime.Now; } }
    }
}