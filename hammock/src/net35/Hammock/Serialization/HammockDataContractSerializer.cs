using System;
using System.Collections.Generic;
#if NET40
using System.Dynamic;
#endif
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
#if !NET20
using System.Xml.Linq;
#endif

namespace Hammock.Serialization
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HammockDataContractSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Dictionary<RuntimeTypeHandle, DataContractSerializer> _serializers =
            new Dictionary<RuntimeTypeHandle, DataContractSerializer>();

#if !SILVERLIGHT
        [NonSerialized]
#endif
        private readonly XmlWriterSettings _settings;

        public HammockDataContractSerializer(XmlWriterSettings settings)
        {
            _settings = settings;
        }

        #region IDeserializer Members

        public virtual object Deserialize(RestResponseBase response, Type type)
        {
            using (var stringReader = new StringReader(response.Content))
            {
                var xmlRoot = XElement.Load(stringReader);
                var serializer = CacheOrGetSerializerFor(type);

                using (var reader = xmlRoot.CreateReader())
                {
                    return serializer.ReadObject(reader);
                }
            }
        }

        public virtual T Deserialize<T>(RestResponseBase response)
        {
            using (var stringReader = new StringReader(response.Content))
            {
                var xmlRoot = XElement.Load(stringReader);
                var serializer = CacheOrGetSerializerFor(typeof (T));

                using (var reader = xmlRoot.CreateReader())
                {
                    return (T) serializer.ReadObject(reader);
                }
            }
        }

#if NET40
        public dynamic DeserializeDynamic(RestResponseBase response)
        {
            throw new NotSupportedException();
        }
#endif

        #endregion

        #region ISerializer Members

        public virtual string Serialize(object instance, Type type)
        {
            string result;

            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, _settings))
                {

                    var serializer = CacheOrGetSerializerFor(type);
                    writer.WriteStartDocument();
                    serializer.WriteObject(writer, instance);
                    writer.Flush();
                }

                var data = stream.ToArray();
                result = ContentEncoding.GetString(data, 0, data.Length);
            }

            return result;
        }

        public virtual string ContentType
        {
            get { return "application/xml"; }
        }

        #endregion

        private DataContractSerializer CacheOrGetSerializerFor(Type type)
        {
            var handle = type.TypeHandle;
            if (_serializers.ContainsKey(handle))
            {
                return _serializers[handle];
            }

            var serializer = new DataContractSerializer(type);
            _serializers.Add(handle, serializer);

            return serializer;
        }
    }
}