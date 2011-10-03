namespace metrics.Core
{
    using System;

    interface IReporter : IDisposable
    {
        void Run();
        void Start(long period, TimeUnit unit);
    }
}
