using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#if NET40
using System.Dynamic;
#endif

namespace Hammock.Serialization
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class HammockXmlSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Dictionary<RuntimeTypeHandle, XmlSerializer> _serializers =
           new Dictionary<RuntimeTypeHandle, XmlSerializer>();

        [NonSerialized]
        private readonly XmlWriterSettings _settings;

        [NonSerialized]
        private readonly XmlSerializerNamespaces _namespaces;

        public HammockXmlSerializer(XmlWriterSettings settings)
        {
            _settings = settings;
        }

        public HammockXmlSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces) : this(settings)
        {
            _namespaces = namespaces;
        }

        #region ISerializer Methods

        public virtual string Serialize(object instance, Type type)
        {
            string result;
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, _settings))
                {
                    var serializer = CacheOrGetSerializerFor(type);

                    if (_namespaces != null)
                    {
                        serializer.Serialize(writer, instance, _namespaces);
                    }
                    else
                    {
                        serializer.Serialize(writer, instance);
                    }
                }

#if !Smartphone && !NETCF
                result = ContentEncoding.GetString(stream.ToArray());
#else
                result = ContentEncoding.GetString(stream.ToArray(), 0, (int)stream.Length);
#endif
            }
            return result;
        }

        #endregion

        public virtual string ContentType
        {
            get { return "application/xml"; }
        }

        #region IDeserializer Methods

        public virtual object Deserialize(RestResponseBase response, Type type)
        {
            object instance;
            var serializer = CacheOrGetSerializerFor(type);
            using(var reader = new StringReader(response.Content))
            {
                instance = serializer.Deserialize(reader);    
            }
            return instance;
        }

        public virtual T Deserialize<T>(RestResponseBase response)
        {
            T instance;
            var serializer = CacheOrGetSerializerFor(typeof(T));
            using (var reader = new StringReader(response.Content))
            {
                instance = (T) serializer.Deserialize(reader);
            }
            return instance;
        }

#if NET40
        public virtual dynamic DeserializeDynamic(RestResponseBase response)
        {
            var result = Deserialize<dynamic>(response);
            return result;
        }
#endif

        #endregion

        private XmlSerializer CacheOrGetSerializerFor(Type type)
        {
            var handle = type.TypeHandle;
            if (_serializers.ContainsKey(handle))
            {
                return _serializers[handle];
            }

            var serializer = new XmlSerializer(type);
            _serializers.Add(handle, serializer);

            return serializer;
        }
    }
}


