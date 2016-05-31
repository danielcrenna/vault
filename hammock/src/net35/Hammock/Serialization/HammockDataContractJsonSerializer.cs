using System;
using System.Collections.Generic;
#if NET40
using System.Dynamic;
#endif
using System.IO;
using System.Runtime.Serialization.Json;

namespace Hammock.Serialization
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HammockDataContractJsonSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Dictionary<RuntimeTypeHandle, DataContractJsonSerializer> _serializers =
            new Dictionary<RuntimeTypeHandle, DataContractJsonSerializer>();

        #region ISerializer Members

        public virtual string Serialize(object instance, Type type)
        {
            string result;
            using (var stream = new MemoryStream())
            {
                var serializer = CacheOrGetSerializerFor(type);
                serializer.WriteObject(stream, instance);

                var data = stream.ToArray();
                result = ContentEncoding.GetString(data, 0, data.Length);
            }
            return result;
        }

        public virtual string ContentType
        {
            get { return "application/json"; }
        }

        #endregion

        #region IDeserializer Members

        public virtual object Deserialize(RestResponseBase response, Type type)
        {
            object instance;
            using (var stream = new MemoryStream(ContentEncoding.GetBytes(response.Content)))
            {
                var serializer = CacheOrGetSerializerFor(type);
                instance = serializer.ReadObject(stream);
            }
            return instance;
        }

        public virtual T Deserialize<T>(RestResponseBase response)
        {
            var type = typeof (T);
            T instance;
            using (var stream = new MemoryStream(ContentEncoding.GetBytes(response.Content)))
            {
                var serializer = CacheOrGetSerializerFor(type);
                instance = (T) serializer.ReadObject(stream);
            }
            return instance;
        }

#if NET40
        public dynamic DeserializeDynamic(RestResponseBase response)
        {
            throw new NotSupportedException();
        }
#endif

        #endregion

        private DataContractJsonSerializer CacheOrGetSerializerFor(Type type)
        {
            var handle = type.TypeHandle;
            if (_serializers.ContainsKey(handle))
            {
                return _serializers[handle];
            }

            var serializer = new DataContractJsonSerializer(type);
            _serializers.Add(handle, serializer);

            return serializer;
        }
    }
}