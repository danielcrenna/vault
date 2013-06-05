using System;
using Newtonsoft.Json;

namespace TweetSharp
{
    // [DC]: All converters must be public for Silverlight to construct them correctly.

    /// <summary>
    /// Sometimes Twitter returns "0" for "true", instead of true, and we've even seen
    /// "13" for true. For those, and possibly future issues, this converter exists.
    /// </summary>
    internal class TwitterWonkyBooleanConverter : TwitterConverterBase
    {
        /// <summary>
        /// Writes the JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is string)
            {
                var boolean = value.Equals("0");
                writer.WriteValue(boolean.ToString());
            }

            if (value is bool)
            {
                writer.WriteValue(value.ToString());
            }
        }

        /// <summary>
        /// Reads the JSON.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = reader.Value.ToString();
            var wonkyBool = value.Equals("0") || !value.Equals("1") && TryConvertBool(value);
            return wonkyBool;
        }

        private static bool TryConvertBool(string value)
        {
            bool result;
            if(bool.TryParse(value, out result))
            {
                return result;
            }
            return false;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            var t = (IsNullableType(objectType))
                        ? Nullable.GetUnderlyingType(objectType)
                        : objectType;

            return typeof (Boolean).IsAssignableFrom(t);
        }
    }
}