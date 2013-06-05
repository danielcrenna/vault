using System;
using Newtonsoft.Json;

namespace TweetSharp
{
    // [DC]: All converters must be public for Silverlight to construct them correctly.

    public class TwitterDateTimeConverter : TwitterConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is TwitterDateTime)
            {
                writer.WriteValue(value.ToString());
            }

            if (value is DateTime)
            {
                var dateTime = (DateTime) value;
                var converted = TwitterDateTime.ConvertFromDateTime(dateTime, TwitterDateFormat.RestApi);

                writer.WriteValue(converted);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = reader.Value.ToString();
            var date = TwitterDateTime.ConvertToDateTime(value);

            return date;
        }

        public override bool CanConvert(Type objectType)
        {
            var t = (IsNullableType(objectType))
                        ? Nullable.GetUnderlyingType(objectType)
                        : objectType;
#if !Smartphone && !NET20
            return typeof (DateTime).IsAssignableFrom(t) || typeof (DateTimeOffset).IsAssignableFrom(t);
#else
            return typeof (DateTime).IsAssignableFrom(t);
#endif
        }
    }
}