using System;
using System.Threading.Tasks;

namespace copper
{
    public interface EventPublisher : IDisposable
    {
        Task<bool> PublishAsync<T>(T @event);
        bool Publish<T>(T @event);
    }
}