using System.IO;
using ServiceStack.Text;

namespace copper.Json
{
    public class JsonSerializer : Serializer
    {
        static JsonSerializer()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
        }

        public void Dispose()
        {
            
        }

        public Stream SerializeToStream<T>(T message)
        {
            var ms = new MemoryStream();
            ServiceStack.Text.JsonSerializer.SerializeToStream(message, ms);
            return ms;
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            var @event = ServiceStack.Text.JsonSerializer.DeserializeFromStream<T>(stream);
            return @event;
        }

        public string SerializeToString<T>(T @event)
        {
            var json = ServiceStack.Text.JsonSerializer.SerializeToString(@event);
            return json;
        }
    }
}
