using System;
using Newtonsoft.Json;

namespace TweetSharp
{
    public abstract class TwitterConverterBase : JsonConverter
    {
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

            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>));
        }
    }
}
