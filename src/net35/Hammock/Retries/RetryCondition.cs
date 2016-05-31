using System;

namespace Hammock.Retries
{
    public abstract class RetryCondition<T> : IRetryCondition<T>
    {
        public abstract Predicate<T> RetryIf { get; }
    }
}