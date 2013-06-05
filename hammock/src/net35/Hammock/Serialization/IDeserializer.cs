using System;
#if NET40
using System.Dynamic;
#endif

namespace Hammock.Serialization
{
    public interface IDeserializer
    {
        object Deserialize(RestResponseBase response, Type type);
        T Deserialize<T>(RestResponseBase response);
#if NET40
        dynamic DeserializeDynamic(RestResponseBase response);
#endif
    }
}