using System;

namespace metrics.Reporting
{
    interface IReporter : IDisposable
    {
        void Run();
        void Start(long period, TimeUnit unit);
    }
}
