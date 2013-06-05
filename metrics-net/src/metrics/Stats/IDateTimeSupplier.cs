using System;

namespace metrics.Stats
{
    internal interface IDateTimeSupplier
    {
        DateTime UtcNow { get; }
    }
}