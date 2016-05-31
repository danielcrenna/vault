using System;
using System.Text;
using Newtonsoft.Json;

namespace Hammock.Tests.Converters
{
    internal class UnicodeJsonStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var buffer = new StringBuilder();
            buffer.Append("\"");
            var stringValue = (string)value;
            foreach (var c in stringValue)
            {
                var code = (int)c;
                switch (c)
                {
                    case '\"':
                        buffer.Append("\\\"");
                        break;
                    case '\\':
                        buffer.Append("\\\\");
                        break;
                    default:
                        if (code > 127)
                        {
                            buffer.AppendFormat("\\u{0:x4}", code);
                        }
                        else
                        {
                            buffer.Append(c);
                        }
                        break;
                }
            }
            buffer.Append("\"");

            writer.WriteRawValue(buffer.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object originalValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}


