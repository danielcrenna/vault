using System;
using Newtonsoft.Json;
#if WINRT
using System.Reflection;
#endif

namespace TweetSharp
{
    public abstract class TwitterConverterBase : JsonConverter
    {
        public static bool IsNullable(Type type)
        {
#if !WINRT
            return type != null && (!type.IsValueType || IsNullableType(type));
#else
					return type != null && (!type.GetTypeInfo().IsValueType || IsNullableType(type));
#endif
        }

        public static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                return false;
            }

#if !WINRT
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>));
#else
						return (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
#endif
        }
    }
}
