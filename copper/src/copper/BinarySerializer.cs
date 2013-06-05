using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace copper
{
    /// <summary>
    /// Serialization provided by the .NET Framework's <see cref="BinaryFormatter" />
    /// </summary>
    public class BinarySerializer : Serializer
    {
        private readonly BinaryFormatter _formatter;

        public BinarySerializer()
        {
            _formatter = new BinaryFormatter();
        }

        public Stream SerializeToStream<T>(T message)
        {
            var ms = new MemoryStream();
            _formatter.Serialize(ms, message);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            return (T) _formatter.Deserialize(stream);
        }
    }
}