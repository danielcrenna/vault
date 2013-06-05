using System.IO;
using ProtoBuf.Meta;

namespace protobuf.Poco
{
    public class Serializer
    {
        private readonly RuntimeTypeModel _model;
        private readonly bool _owner;

        public Serializer()
        {
            _model = ReflectionRuntimeTypeModel.Create();
            _owner = true;
        }

        public Serializer(RuntimeTypeModel model)
        {
            _model = model;
            _owner = false;
        }
        
        public Stream SerializeToStream<T>(T @event)
        {
            if(_owner) { EnsureTypeCanSerialize<T>(); }
            var stream = new MemoryStream();
            _model.Serialize(stream, @event);
            stream.Position = 0;
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
