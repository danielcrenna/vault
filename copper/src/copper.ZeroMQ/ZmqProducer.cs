using System;
using System.Collections.Generic;
using System.IO;
using copper.Extensions;

namespace copper.ZeroMQ
{
    /// <summary>
    /// A producer that listens on a ZMQ socket. Suitable for transport.
    /// </summary>
    public class ZmqProducer : UsesBackgroundProducer<Stream>
    {
        private ZmqSubscriber _subscriber;
        
        public ZmqProducer(string endpoint)
        {
            _subscriber = new ZmqSubscriber(endpoint);
            _subscriber.ConnectSocket();

            Background.Produce(new Func<IEnumerable<Stream>>(YieldFromSocket).AsContinuousObservable());
        }

        private IEnumerable<Stream> YieldFromSocket()
        {
            Stream item;
            if (TryDequeue(out item))
            {
                yield return item;
            }
        }

        private bool TryDequeue(out Stream item)
        {
            var buffer = _subscriber.Receive();
            if (buffer == null || buffer.Length == 0)
            {
                item = null;
                return false;
            }
            item = new MemoryStream(buffer);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            if (_subscriber == null)
            {
                return;
            }
            _subscriber.Dispose();
            _subscriber = null;
        }
    }

    /// <summary>
    /// A producer that listens on a ZMQ socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZmqProducer<T> : UsesBackgroundProducer<T>
    {
        private ZmqSubscriber _subscriber;
        private readonly Serializer _serializer;

        public ZmqProducer(string endpoint) : this(endpoint, new BinarySerializer())
        {
            
        }

        public ZmqProducer(string endpoint, Serializer serializer)
        {
            _subscriber = new ZmqSubscriber(endpoint);
            _subscriber.ConnectSocket();
            _serializer = serializer;

            Background.Produce(new Func<IEnumerable<T>>(YieldFromSocket).AsContinuousObservable());
        }

        private IEnumerable<T> YieldFromSocket()
        {
            T item;
            if (TryDequeue(out item))
            {
                yield return item;
            }
        }

        private bool TryDequeue(out T item)
        {
            var buffer = _subscriber.Receive();
            if (buffer.Length == 0)
            {
                item = default(T);
                return false;
            }
            var ms = new MemoryStream(buffer);
            var @event = _serializer.DeserializeFromStream<T>(ms);
            item = @event;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            if (_subscriber == null)
            {
                return;
            }
            _subscriber.Dispose();
            _subscriber = null;
        }
    }
}