using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading;
using copper.Extensions;

namespace copper
{
    /// <summary>
    /// A producer that feeds its consumer continuously from a collection. The collection itself
    /// is consumed, not enumerated, when the producer produces from it. Additions to the 
    /// underlying collection are picked up by the producer.
    /// </summary>
    public class CollectionProducer<T> : UsesBackgroundProducer<T> where T : class
    {
        private readonly CancellationTokenSource _cancel;

        public CollectionProducer(IProducerConsumerCollection<T> queue) : this(queue, TimeSpan.FromSeconds(1000))
        {
            
        }

        public CollectionProducer(IProducerConsumerCollection<T> queue, TimeSpan interval) : this()
        {
            Background.Produce(new Func<T>(() => TryDequeue(queue))
                .AsContinuousObservable()
                .Where(e => e != null)
                .Buffer(interval));
        }

        private static T TryDequeue(IProducerConsumerCollection<T> queue)
        {
            T @event;
            var dequeued = queue.TryTake(out @event);
            if(dequeued)
            {
                return @event;
            }
            return null;
        }

        public CollectionProducer(BlockingCollection<T> collection) : this()
        {
            Background.Produce(collection.AsConsumingObservable(_cancel.Token));
        }

        private CollectionProducer()
        {
            _cancel = new CancellationTokenSource();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            if (_cancel != null)
            {
                _cancel.Cancel();
                _cancel.Token.WaitHandle.WaitOne();
            }
        }
    }
}