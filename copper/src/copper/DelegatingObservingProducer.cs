using System;
using System.Threading;
using copper.Extensions;

namespace copper
{
    /// <summary>
    /// A producer that emits the results of a delegate, converting it to an observable.
    /// The delegate is continously invoked until cancelled internally. In other words,
    /// it can be used to produce continuous sequences.
    /// <remarks>
    /// It's helpful to use Rx's Window or Buffer extension methods to reduce busy-waiting on
    /// these continous sequences.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegatingObservingProducer<T> : Produces<T>, IDisposable
    {
        private ObservingProducer<T> _producer;

        public DelegatingObservingProducer(Func<T> @delegate)
        {
            _producer = new ObservingProducer<T>();
            _producer.Produces(@delegate.AsContinuousObservable());
        }

        public DelegatingObservingProducer(Func<CancellationToken, T> @delegate)
        {
            _producer = new ObservingProducer<T>();
            _producer.Produces(@delegate.AsContinuousObservable());
        }
        
        public void Start()
        {
            _producer.Start();
        }

        public void Attach(Consumes<T> consumer)
        {
            _producer.Attach(consumer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_producer == null) return;
            _producer.Dispose();
            _producer = null;
        }
    }
}