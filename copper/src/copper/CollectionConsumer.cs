using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace copper
{
    /// <summary>
    /// A consumer that forwards all handled events to an in-memory blocking collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionConsumer<T> : AsyncConsumer<T>, IEnumerable<T>, IDisposable
    {
        private BlockingCollection<T> _collection;

        public CollectionConsumer()
        {
            _collection = new BlockingCollection<T>();
        }

        public CollectionConsumer(IProducerConsumerCollection<T> collection)
        {
            _collection = new BlockingCollection<T>(collection);
        }

        public CollectionConsumer(BlockingCollection<T> collection)
        {
            _collection = collection;
        }

        public override bool Handle(T @event)
        {
            return _collection.TryAdd(@event);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetConsumingEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            if (_collection == null) return;
            _collection.Dispose();
            _collection = null;
        }
    }
}