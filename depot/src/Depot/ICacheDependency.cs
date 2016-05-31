using System;

namespace depot
{
    public interface ICacheDependency : IDisposable
    {
        string Id { get; }
        bool ShouldInvalidate { get; }
        DateTime LastModified { get; }
    }
}