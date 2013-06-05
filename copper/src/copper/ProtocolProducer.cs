using System;
using System.IO;
using System.Threading.Tasks;

namespace copper
{
    /// <summary>
    /// The production end of a pipe that abstracts away the serialization mechanism from downstream consumers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProtocolProducer<T> : Pipe<Stream, T>
    {
        private readonly Serializer _serializer;
        private Func<Stream, bool> _handler;
        private static Func<Stream, bool> DefaultHandler()
        {
            return @default => true;
        }
        
        public ProtocolProducer() : this(new BinarySerializer()) { }
        
        public ProtocolProducer(Serializer serializer)
        {
            _serializer = serializer;
            _handler = DefaultHandler();
        }

        public void Attach(Consumes<Stream> consumer)
        {
            _handler = consumer.Handle;
        }

        public async Task<bool> HandleAsync(T @event)
        {
            var serialized = _serializer.SerializeToStream(@event);
            return await Task.Factory.StartNew(() =>
            {
                _handler(serialized);
                return true;
            });
        }

        public bool Handle(T @event)
        {
            var serialized = _serializer.SerializeToStream(@event);
            return _handler(serialized);
        }
    }
}