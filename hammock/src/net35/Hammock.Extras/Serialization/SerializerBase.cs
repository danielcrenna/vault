using System;
#if NET40
using System.Dynamic;
#endif
using System.IO;
using System.Xml;
using Hammock.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Hammock.Extras.Serialization
{
    public abstract class SerializerBase : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        protected SerializerBase()
            : this(new JsonSerializerSettings
                       {
                           MissingMemberHandling = MissingMemberHandling.Ignore,
                           NullValueHandling = NullValueHandling.Ignore,
                           DefaultValueHandling = DefaultValueHandling.Include,
                           ContractResolver = new JsonConventionResolver()
                       })
        {

        }

        protected SerializerBase(JsonSerializerSettings settings)
        {
            _serializer = new Newtonsoft.Json.JsonSerializer
                              {
                                  ConstructorHandling = settings.ConstructorHandling,
                                  ContractResolver = settings.ContractResolver,
                                  ObjectCreationHandling = settings.ObjectCreationHandling,
                                  MissingMemberHandling = settings.MissingMemberHandling,
                                  DefaultValueHandling = settings.DefaultValueHandling,
                                  NullValueHandling = settings.NullValueHandling
                              };

            foreach (var converter in settings.Converters)
            {
                _serializer.Converters.Add(converter);
            }
        }

        public abstract T Deserialize<T>(RestResponse<T> response);

        public abstract object Deserialize(RestResponse response, Type type);

#if NET40
        public abstract dynamic DeserializeDynamic<T>(RestResponse<T> response) where T : DynamicObject;
#endif
        
        public virtual object DeserializeJson(string content, Type type)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize(jsonTextReader, type);
                }
            }
        }

        public virtual T DeserializeJson<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public virtual T DeserializeXml<T>(RestResponse response)
        {
            return (T)DeserializeXml(response, typeof(T));
        }

        public virtual object DeserializeXml(RestResponse response, Type type)
        {
            var element = LoadXmlElement(response.Content);

            return DeserializeXmlImpl(element, type);
        }

        private object DeserializeXmlImpl(XmlNode node, Type type)
        {
            var json = JsonConvert.SerializeXmlNode(node);
            var instance = DeserializeJson(json, type);
            return instance;
        }

        private static XmlElement LoadXmlElement(string content)
        {
            var document = new XmlDocument();
            document.Load(new StringReader(content));
            return document.DocumentElement;
        }

        public virtual object DeserializeXmlWithRoot(string content, Type type, string root)
        {
            var source = LoadXmlElement(content);
            var inner = source.FirstChild;

            var document = new XmlDocument();
            var outer = document.CreateNode(XmlNodeType.Element, root, source.NamespaceURI);
            inner = document.ImportNode(inner, true);

            foreach(XmlAttribute attribute in source.Attributes)
            {
                var attributeCopy = document.CreateAttribute(attribute.Name, attribute.LocalName, attribute.NamespaceURI);
                attributeCopy.Value = attribute.Value;
                if (outer.Attributes != null)
                {
                    outer.Attributes.Append(attributeCopy);
                }
            }

            outer.AppendChild(inner);
            document.AppendChild(outer);

            var json = JsonConvert.SerializeXmlNode(document);
            var relevant = JObject.Parse(json)[root].ToString();
            var instance = DeserializeJson(relevant, type);
            
            return instance;
        }

        public virtual string SerializeJson(object instance, Type type)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, instance);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        public virtual string SerializeXml(object instance, Type type)
        {
            var json = SerializeJson(instance, type);

            var root = type.Name.ToLowerInvariant();

            return SerializeXmlImpl(instance, type, json, root);
        }

        public virtual string SerializeXmlWithRoot(object instance, Type type, string root)
        {
            var json = SerializeJson(instance, type);

            return SerializeXmlImpl(instance, type, json, root);
        }

        public virtual string SerializeXmlImpl(object instance, Type type, string json, string root)
        {
            // {"root":json }
            json = string.Format("{{\"{0}\":{1} }}", root, json);

            var document = JsonConvert.DeserializeXmlNode(json);

            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = new XmlTextWriter(stringWriter))
                {
                    document.WriteTo(xmlTextWriter);

                    return stringWriter.ToString();
                }
            }
        }

        public abstract string Serialize(object instance, Type type);

        public abstract string ContentType { get; }
    }
}