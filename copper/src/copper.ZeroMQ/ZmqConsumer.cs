using System.IO;
using ZeroMQ;
using copper.Extensions;

namespace copper.ZeroMQ
{
    /// <summary>
    /// Consumes a data stream and pushes it through a ZMQ socket
    /// </summary>
    public class ZmqConsumer : AsyncConsumer<Stream>
    {
        private ZmqPublisher _publisher;

        public ZmqConsumer(string endpoint)
        {
            _publisher = new ZmqPublisher(endpoint);
        }

        public override bool Handle(Stream @event)
        {
            var data = @event.ReadFully();
            return _publisher.Send(data) == SendStatus.Sent;
        }

        public override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            _publisher.Dispose();
            _publisher = null;
        }
    }
    
    /// <summary>
    /// Consumes events and serializes to a ZMQ socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZmqConsumer<T> : AsyncConsumer<T>
    {
        private readonly Serializer _serializer;
        private ZmqPublisher _publisher;

        public ZmqConsumer(string endpoint) : this(endpoint, new BinarySerializer())
        {
            
        }

        public ZmqConsumer(string endpoint, Serializer serializer)
        {
            _serializer = serializer;
            _publisher = new ZmqPublisher(endpoint);
        }

        public override bool Handle(T @event)
        {
            var serialized = _serializer.SerializeToStream(@event);
            using (var ms = new MemoryStream())
            {
                serialized.CopyTo(ms);
                return _publisher.Send(ms.ToArray()) == SendStatus.Sent;
            }
        }

        public override void Dispose(bool disposing)
        {
            if (!disposing || _publisher == null)
            {
                return;
            }
            _publisher.Dispose();
            _publisher = null;
        }
    }
}
