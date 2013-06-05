using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace hammock2
{
    public partial class HttpBody
    {
        static HttpBody()
        {
            Converter = new ServiceStackJsonConverter();
        }
    }

    public class ServiceStackJsonConverter : IMediaConverter
    {
        static ServiceStackJsonConverter()
        {
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
        }
        public string DynamicToString(dynamic instance)
        {
            var @string = JsonSerializer.SerializeToString(instance);
            return @string;
        }
        public IDictionary<string, object> StringToHash(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object>();
            var hash = JsonSerializer.DeserializeFromString<JsonObject>(json);
            var result = hash.ToDictionary<KeyValuePair<string, string>, string, object>(entry => entry.Key, entry => entry.Value);
            return result;
        }
        public string HashToString(IDictionary<string, object> hash)
        {
            var @string = JsonSerializer.SerializeToString(hash);
            return @string;
        }
        public T DynamicTo<T>(dynamic instance)
        {
            var @string = instance.ToString();
            return StringTo<T>(@string); // <-- Two pass, could be faster
        }
        public T StringTo<T>(string instance)
        {
            return JsonSerializer.DeserializeFromString<T>(instance);
        }
    }

    public class JsonMediaTypeFormatter : MediaTypeFormatter
    {
        public JsonMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            SupportedMediaTypes.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task<object>.Factory.StartNew(() => JsonSerializer.DeserializeFromStream(type, readStream));
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() => JsonSerializer.SerializeToStream(value, type, writeStream));
        }
    }
}
