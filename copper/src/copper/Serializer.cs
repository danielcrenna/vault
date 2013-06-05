using System.IO;

namespace copper
{
    public interface Serializer 
    {
        Stream SerializeToStream<T>(T @event);
        T DeserializeFromStream<T>(Stream stream);
    }
}