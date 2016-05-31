using System;

namespace Hammock.Tasks
{
    public interface ITaskOptions<T>
    {
        double? RateLimitPercent { get; set; }
        RateLimitType RateLimitType { get; }
        Predicate<T> RateLimitingPredicate { get; }
        Func<T> GetRateLimitStatus { get; }
    }

    public interface ITaskOptions
    {
        TimeSpan DueTime { get; set; }
        int RepeatTimes { get; set; }
        TimeSpan RepeatInterval { get; set; }
        bool ContinueOnError { get; set; }
    }
}