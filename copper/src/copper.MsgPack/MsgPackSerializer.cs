using System.IO;
using MsgPack;

namespace copper.MsgPack
{
    public class MsgPackSerializer : Serializer
    {
        private readonly ObjectPacker _packer;

        public MsgPackSerializer()
        {
            _packer = new ObjectPacker();
        }

        public void Dispose()
        {
            
        }

        public Stream SerializeToStream<T>(T message)
        {
            var buffer = _packer.Pack(message);
            return new MemoryStream(buffer);
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            return _packer.Unpack<T>(stream);
        }
    }
}
