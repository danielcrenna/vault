using System.IO;
using ProtoBuf.Meta;

namespace copper.ProtocolBuffers
{
    public class ProtocolBuffersSerializer : Serializer
    {
        private readonly RuntimeTypeModel _model;
        private readonly bool _owner;

        public ProtocolBuffersSerializer()
        {
            _model = ReflectionRuntimeTypeModel.Create();
            _owner = true;
        }

        public ProtocolBuffersSerializer(RuntimeTypeModel model)
        {
            _model = model;
            _owner = false;
        }
        
        public Stream SerializeToStream<T>(T @event)
        {
            if(_owner) { EnsureTypeCanSerialize<T>(); }
            var stream = new MemoryStream();
            _model.Serialize(stream, @event);
            return stream;
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            if (_owner) { EnsureTypeCanSerialize<T>(); }
            return (T)_model.Deserialize(stream, null, typeof(T));
        }

        private void EnsureTypeCanSerialize<T>()
        {
            if (!_model.CanSerialize(typeof(T)))
            {
                ReflectionRuntimeTypeModel.AddType<T>(_model);
            }
        }
    }
}
