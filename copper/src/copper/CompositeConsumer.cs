using System.Collections.Generic;
using System.Linq;

namespace copper
{
    /// <summary>
    /// A <see cref="Consumes{T}"/> that allows for attaching multiple consumers to a single producer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompositeConsumer<T> : AsyncConsumer<T>
    {
        private readonly IList<Consumes<T>> _consumers;

        public CompositeConsumer()
        {
            _consumers = new List<Consumes<T>>();
        }

        public CompositeConsumer(IEnumerable<Consumes<T>> consumers)
        {
            _consumers = new List<Consumes<T>>(consumers);
        }

        public override bool Handle(T @event)
        {
            return _consumers.Aggregate(true, (current, consumer) => current & consumer.Handle(@event));
        }

        public void Add(Consumes<T> consumer)
        {
            _consumers.Add(consumer);
        }
    }
}