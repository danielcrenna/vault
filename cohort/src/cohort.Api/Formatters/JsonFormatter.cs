using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace cohort.API.Formatters
{
    public class JsonFormatter : MediaTypeFormatter
    {
        static JsonFormatter()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
        }

        public JsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/json"));
            SupportedEncodings.Add(new UTF8Encoding(false, true));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() => JsonSerializer.DeserializeFromStream(type, readStream));
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() => JsonSerializer.SerializeToStream(value, writeStream));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }
    }
}