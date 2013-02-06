using System;

namespace metrics.Stats
{
    internal interface IDateTimeSupplier
    {
        DateTime Now { get; }
    }
}