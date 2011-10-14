using System;

namespace metrics.Core
{
    interface IReporter : IDisposable
    {
        void Run();
        void Start(long period, TimeUnit unit);
    }
}
