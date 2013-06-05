using System;
using System.Threading.Tasks;

namespace copper
{
    /// <summary>
    /// Only requires implementing the sequential verson of the consumer to get the asynchronous version for free
    /// </summary>
    public abstract class AsyncConsumer<T> : Consumes<T>
    {
        private readonly Func<T, Task<bool>> _handler;
        
        private static Func<T, Task<bool>> DefaultCallback()
        {   
            return @default => Task.Factory.StartNew(() => true);
        }
        
        protected AsyncConsumer()
        {
            _handler = DefaultCallback();
        }
        
        public async Task<bool> HandleAsync(T @event)
        {
            return await Perform(@event);
        }
        
        private async Task<bool> Perform(T message)
        {
            Handle(message);
            return await _handler(message);
        }
        
        public abstract bool Handle(T @event);
        
        public virtual void Dispose(bool disposing)
        {
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}