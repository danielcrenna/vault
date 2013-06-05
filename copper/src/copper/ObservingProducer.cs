using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace copper
{
    /// <summary>
    /// A producer that emits the results of <see cref="IObservable{T}"/> sequences
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservingProducer<T> : Produces<T>, IDisposable
    {
        private Consumes<T> _consumer;
        private readonly IList<IDisposable> _handles;
        private readonly IDictionary<IObserver<T>, IObservable<T>> _cache;
        
        public ObservingProducer()
        {
            _cache = new ConcurrentDictionary<IObserver<T>, IObservable<T>>();
            _handles = new List<IDisposable>();
        }

        public virtual ObservingProducer<T> Produces(IEnumerable<T> sequence, Action<Exception> onError = null, Action onCompleted = null)
        {
            return Produces(sequence.ToObservable(), onError, onCompleted);
        }

        public virtual ObservingProducer<T> Produces(IObservable<T> sequence, Action<Exception> onError = null, Action onCompleted = null)
        {
            var observer = new DelegatingObserver<T>(@event => _consumer.Handle(@event), onError ?? (exception => { }), onCompleted ?? (()=> { }));
            _cache.Add(observer, sequence);
            return this;
        }

        public void Start()
        {
            SubscribeFromCache();
        }
        
        private void SubscribeFromCache()
        {
            foreach (var item in _cache)
            {
                _handles.Add(item.Value.Subscribe(item.Key));
            }
        }

        public void Attach(Consumes<T> consumer)
        {
            _consumer = consumer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            foreach(var handle in _handles)
            {
                handle.Dispose();
            }
            _handles.Clear();
            _cache.Clear();
        }
    }
}