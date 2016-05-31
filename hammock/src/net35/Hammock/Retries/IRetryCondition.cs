using System;

namespace Hammock.Retries
{
    public interface IRetryCondition
    {
        
    }

    public interface IRetryCondition<T> : IRetryCondition
    {
        Predicate<T> RetryIf { get; }
    }
}