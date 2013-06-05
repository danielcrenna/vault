using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace copper
{
    /// <summary>
    /// Allows subscribing handlers to centralized publishing events, and allows subscription by topic
    /// </summary>
    public class Hub : EventAggregator, EventPublisher
    {
        private readonly ConcurrentDictionary<Type, WeakReference> _subscriptions = new ConcurrentDictionary<Type, WeakReference>();
        private readonly ConcurrentDictionary<Type, CancellationTokenSource> _unsubscriptions = new ConcurrentDictionary<Type, CancellationTokenSource>();

        public async Task<bool> PublishAsync<T>(T @event)
        {
            return await Task.Factory.StartNew(()=> Publish(@event));
        }

        public bool Publish<T>(T @event)
        {
            WeakReference subscription;
            if (_subscriptions.TryGetValue(typeof (T), out subscription))
            {
                Box<T>(subscription).OnNext(@event);
                return true;
            }
            return false;
        }

        public void Subscribe<T>(Action<T> @handler)
        {
            var subscription = GetSubscriptionSubject<T>();
            var observable = Box<T>(subscription).AsObservable();
            observable.Subscribe(@handler);
        }

        public void Subscribe<T>(Action<T> @handler, Func<T, bool> topic)
        {
            var subscription = GetSubscriptionSubject<T>();
            var observable = Box<T>(subscription).Where(topic).AsObservable();
            observable.Subscribe(@handler);
        }
        
        public void Subscribe<T>(Consumes<T> consumer)
        {
            var subscription = GetSubscriptionSubject<T>();
            var observable = Box<T>(subscription).AsObservable();
            var unsubscription = _unsubscriptions.GetOrAdd(typeof (T), t => new CancellationTokenSource());
            observable.Subscribe(@event => consumer.Handle(@event), exception => { }, () => { }, unsubscription.Token);
        }

        public void Subscribe<T>(Consumes<T> consumer, Func<T, bool> topic)
        {
            var subscription = GetSubscriptionSubject<T>();
            var observable = Box<T>(subscription).Where(topic).AsObservable();
            observable.Subscribe(@event => consumer.Handle(@event), exception => { }, () => { });
        }

        public void Unsubscribe<T>()
        {
            WeakReference reference;
            _subscriptions.TryRemove(typeof (T), out reference);

            CancellationTokenSource cancel;
            if(_unsubscriptions.TryGetValue(typeof(T), out cancel))
            {
                cancel.Cancel();
            }
        }

        private object GetSubscriptionSubject<T>()
        {
            return _subscriptions.GetOrAdd(typeof(T), t => new WeakReference(new Subject<T>()));
        }
        
        private static ISubject<T> Box<T>(object subscription)
        {
            var reference = ((WeakReference) subscription).Target;
            return ((ISubject<T>)reference);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposing || _subscriptions == null || _subscriptions.Count == 0)
            {
                return;
            }
            foreach (var subscription in _subscriptions.Where(subscription => subscription.Value.Target is IDisposable))
            {
                (((IDisposable)subscription.Value.Target)).Dispose();
            }
        }
    }
}