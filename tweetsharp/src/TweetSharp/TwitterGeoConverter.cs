using System;
using System.Globalization;
using Newtonsoft.Json;

namespace TweetSharp
{
    // [DC]: All converters must be public for Silverlight to construct them correctly.
    
    public class TwitterRateLimitResourceConverter : TwitterConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var result = new TwitterRateLimitStatusSummary();
            reader.Read();
            reader.Read();
            reader.Read();
            result.AccessToken = reader.ReadAsString(); // access_token
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            var t = (IsNullableType(objectType))
                        ? Nullable.GetUnderlyingType(objectType)
                        : objectType;

            return typeof(TwitterRateLimitStatusSummary).IsAssignableFrom(t);
        }
    }

    /// <summary>
    /// This converter exists to convert geo-spatial coordinates.
    /// </summary>
    internal class TwitterGeoConverter : TwitterConverterBase
    {
        private const string GeoTemplate = "\"geo\":{{\"type\":\"Point\",\"coordinates\":[{0}, {1}]}}";

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (!(value is TwitterGeoLocation.GeoCoordinates))
            {
                return;
            }

            var location = (TwitterGeoLocation.GeoCoordinates)value;
            var latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);
            var json = string.Format(GeoTemplate, latitude, longitude);
            writer.WriteRawValue(json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                return null;
            }
            reader.Read();
            var coords = new double[2];
            for (int i = 0; i < 2; ++i)
            {
                if (reader.TokenType == JsonToken.Float)
                {
                    coords[i] = (double)reader.Value;
                    reader.Read();
                }
                else if (reader.TokenType == JsonToken.Integer)
                {
                    coords[i] = (double)((long)reader.Value);
                    reader.Read();
                }
            }

            var latitude = coords[0];
            var longitude = coords[1];

            return new TwitterGeoLocation.GeoCoordinates { Latitude = latitude, Longitude = longitude };
        }

        public override bool CanConvert(Type objectType)
        {
            var t = (IsNullableType(objectType))
                        ? Nullable.GetUnderlyingType(objectType)
                        : objectType;

            return typeof(TwitterGeoLocation.GeoCoordinates).IsAssignableFrom(t);
        }
    }
}
