using System;

namespace copper
{
    /// <summary>
    /// A consumer that executes a delegate against any received events.
    /// It can also optionally forward to another consumer after invoking the delegate action.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegatingConsumer<T> : AsyncConsumer<T>
    {
        private readonly Func<T, bool> _delegate;

        public DelegatingConsumer(Action<T> @delegate, Consumes<T> forwardTo)
        {
            _delegate = @event =>
            {
                try
                {
                    @delegate(@event);
                    return forwardTo.Handle(@event);
                }
                catch (Exception)
                {
                    return false;
                }
            };
        }

        public DelegatingConsumer(Action<T> @delegate)
        {
            _delegate = arg =>
            {
                try
                {
                    @delegate(arg);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            };
        }

        public DelegatingConsumer(Func<T, bool> @delegate)
        {
            _delegate = @delegate;
        }
        
        public override bool Handle(T @event)
        {
            return _delegate(@event);
        }
    }
}