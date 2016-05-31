using System;

namespace Hammock.Tasks
{
    public interface ITimedTask : IDisposable
    {
        Action<bool> Action { get; }
        Exception Exception { get; }

        TimeSpan DueTime { get; }
        TimeSpan Interval { get; }

        void Start();
        void Start(TimeSpan dueTime, TimeSpan interval);
        void Stop();
    }

    public interface ITimedTask<T> : ITimedTask
    {
        bool RateLimited { get; }
        IRateLimitingRule<T> RateLimitingRule { get; }
    }
}