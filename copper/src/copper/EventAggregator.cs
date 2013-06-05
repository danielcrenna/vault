using System;

namespace copper
{
    public interface EventAggregator
    {
        void Subscribe<T>(Action<T> @handler);
        void Subscribe<T>(Action<T> @handler, Func<T, bool> topic);
        void Subscribe<T>(Consumes<T> consumer);
        void Subscribe<T>(Consumes<T> consumer, Func<T, bool> topic);
        void Unsubscribe<T>();
    }
}