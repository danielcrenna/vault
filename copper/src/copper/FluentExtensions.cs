using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace copper
{
    /// <summary>
    /// Contains extension methods to reduce verbosity when using any of the built-in producers 
    /// to attach any of the built-in consumers.
    /// </summary>
    public static class FluentExtensions
    {
        #region Produces<T>
        public static Produces<T> Consumes<T>(this Produces<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static Produces<T> Consumes<T>(this Produces<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static Produces<T> Consumes<T>(this Produces<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static Produces<T> Consumes<T>(this Produces<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static Produces<T> Consumes<T>(this Produces<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        
        #endregion

        #region CollectionProducer<T>
        public static CollectionProducer<T> Consumes<T>(this CollectionProducer<T> producer, Consumes<T> consumer) where T : class
        {
            producer.Attach(consumer);
            return producer;
        }
        public static CollectionProducer<T> Consumes<T>(this CollectionProducer<T> producer, Action<T> @delegate) where T : class 
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static CollectionProducer<T> Consumes<T>(this CollectionProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo) where T : class
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static CollectionProducer<T> Consumes<T>(this CollectionProducer<T> producer, IEnumerable<Consumes<T>> consumers) where T : class
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        #endregion
        
        #region ObservingProducer<T>
        public static ObservingProducer<T> Consumes<T>(this ObservingProducer<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static ObservingProducer<T> Consumes<T>(this ObservingProducer<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static ObservingProducer<T> Consumes<T>(this ObservingProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static ObservingProducer<T> Consumes<T>(this ObservingProducer<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static ObservingProducer<T> Consumes<T>(this ObservingProducer<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion

        #region BackgroundProducer<T>
        public static BackgroundProducer<T> Consumes<T>(this BackgroundProducer<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static BackgroundProducer<T> Consumes<T>(this BackgroundProducer<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static BackgroundProducer<T> Consumes<T>(this BackgroundProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static BackgroundProducer<T> Consumes<T>(this BackgroundProducer<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static BackgroundProducer<T> Consumes<T>(this BackgroundProducer<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion

        #region FileProducer<T>
        public static FileProducer<T> Consumes<T>(this FileProducer<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static FileProducer<T> Consumes<T>(this FileProducer<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static FileProducer<T> Consumes<T>(this FileProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static FileProducer<T> Consumes<T>(this FileProducer<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static FileProducer<T> Consumes<T>(this FileProducer<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion

        #region UsesBackgroundProducer<T>
        public static UsesBackgroundProducer<T> Consumes<T>(this UsesBackgroundProducer<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static UsesBackgroundProducer<T> Consumes<T>(this UsesBackgroundProducer<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static UsesBackgroundProducer<T> Consumes<T>(this UsesBackgroundProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static UsesBackgroundProducer<T> Consumes<T>(this UsesBackgroundProducer<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static UsesBackgroundProducer<T> Consumes<T>(this UsesBackgroundProducer<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion

        #region DelegatingObservingProducer<T>
        public static DelegatingObservingProducer<T> Consumes<T>(this DelegatingObservingProducer<T> producer, Consumes<T> consumer)
        {
            producer.Attach(consumer);
            return producer;
        }
        public static DelegatingObservingProducer<T> Consumes<T>(this DelegatingObservingProducer<T> producer, Action<T> @delegate)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static DelegatingObservingProducer<T> Consumes<T>(this DelegatingObservingProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo)
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static DelegatingObservingProducer<T> Consumes<T>(this DelegatingObservingProducer<T> producer, IEnumerable<Consumes<T>> consumers)
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static DelegatingObservingProducer<T> Consumes<T>(this DelegatingObservingProducer<T> producer, IProducerConsumerCollection<T> collection)
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion

        #region MessageQueueProducer<T>
        public static MessageQueueProducer<T> Consumes<T>(this MessageQueueProducer<T> producer, Consumes<T> consumer) where T : class
        {
            producer.Attach(consumer);
            return producer;
        }
        public static MessageQueueProducer<T> Consumes<T>(this MessageQueueProducer<T> producer, Action<T> @delegate) where T : class
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate));
            return producer;
        }
        public static MessageQueueProducer<T> Consumes<T>(this MessageQueueProducer<T> producer, Action<T> @delegate, Consumes<T> forwardTo) where T : class
        {
            producer.Attach(new DelegatingConsumer<T>(@delegate, forwardTo));
            return producer;
        }
        public static MessageQueueProducer<T> Consumes<T>(this MessageQueueProducer<T> producer, IEnumerable<Consumes<T>> consumers) where T : class
        {
            producer.Attach(new CompositeConsumer<T>(consumers));
            return producer;
        }
        public static MessageQueueProducer<T> Consumes<T>(this MessageQueueProducer<T> producer, IProducerConsumerCollection<T> collection) where T : class
        {
            producer.Attach(new CollectionConsumer<T>(collection));
            return producer;
        }
        #endregion
    }
}