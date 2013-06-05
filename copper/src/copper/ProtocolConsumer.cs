using System;
using System.IO;
using System.Threading.Tasks;

namespace copper
{
    /// <summary>
    /// The consumption end of a pipe that abstracts away the serialization mechanism from downstream consumers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProtocolConsumer<T> : Pipe<T, Stream>
    {
        private readonly Serializer _serializer;

        private Func<T, bool> _handler;
        private static Func<T, bool> DefaultHandler()
        {
            return @default => true;
        }
        
        public ProtocolConsumer() : this(new BinarySerializer()) { }
        
        public ProtocolConsumer(Serializer serializer)
        {
            _serializer = serializer;
            _handler = DefaultHandler();
        }

        public async Task<bool> HandleAsync(Stream stream)
        {
            var deserialized = _serializer.DeserializeFromStream<T>(stream);
            return await Task.Factory.StartNew(() =>
            {
                _handler(deserialized);
                return true;
            });
        }

        public void Attach(Consumes<T> consumer)
        {
            _handler = consumer.Handle;
        }

        public bool Handle(Stream @stream)
        {
            var deserialized = _serializer.DeserializeFromStream<T>(stream);
            return _handler(deserialized);
        }
    }
}