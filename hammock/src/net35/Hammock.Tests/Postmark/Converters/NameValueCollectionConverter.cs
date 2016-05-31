using System;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;

namespace Hammock.Tests.Postmark.Converters
{
    internal class NameValuePair
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    internal class NameValueCollectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is NameValueCollection))
            {
                return;
            }

            var collection = (NameValueCollection)value;
            var container = collection.AllKeys.Select(key => new NameValuePair
                                                                 {
                                                                     Name = key,
                                                                     Value = collection[key]
                                                                 }).ToList();

            var serialized = JsonConvert.SerializeObject(container);

            writer.WriteRawValue(serialized);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object originalValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            var t = (IsNullableType(objectType))
                        ? Nullable.GetUnderlyingType(objectType)
                        : objectType;

            return typeof(NameValueCollection).IsAssignableFrom(t);
        }

        public static bool IsNullable(Type type)
        {
            return type != null && (!type.IsValueType || IsNullableType(type));
        }

        public static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}


