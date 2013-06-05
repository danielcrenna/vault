using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace copper
{
    public class XmlSerializer : Serializer
    {
        private static readonly IDictionary<Type, System.Xml.Serialization.XmlSerializer> Cache;
        private static readonly XmlSerializerNamespaces IgnoreNamespace;

        public bool IgnoreNamespaces { get; set; }
        public bool IgnoreXmlDeclaration { get; set; }

        static XmlSerializer()
        {
            Cache = new Dictionary<Type, System.Xml.Serialization.XmlSerializer>();
            IgnoreNamespace = new XmlSerializerNamespaces();
            IgnoreNamespace.Add("", "");
        }

        public Stream SerializeToStream<T>(T @event)
        {
            return SerializeToStream(typeof (T), @event);
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            return (T) DeserializeFromStream(typeof(T), stream);
        }

        public Stream SerializeToStream(Type type, object instance)
        {
            var stream = new MemoryStream();
            var serializer = GetOrCreateSerializer(type);
            var streamWriter = new StreamWriter(stream);
            var writer = IgnoreXmlDeclaration ? new IgnoreXmlDeclarationWriter(streamWriter) : new XmlTextWriter(streamWriter);
            serializer.Serialize(writer, instance, IgnoreNamespace);
            return stream;
        }

        public object DeserializeFromStream(Type type, Stream stream)
        {
            var serializer = GetOrCreateSerializer(type);
            var streamReader = new StreamReader(stream);
            var reader = IgnoreNamespaces ? new IgnoreNamespacesReader(streamReader) : new XmlTextReader(streamReader);
            var instance = serializer.Deserialize(reader);
            return instance;
        }

        private static System.Xml.Serialization.XmlSerializer GetOrCreateSerializer(Type type)
        {
            System.Xml.Serialization.XmlSerializer serializer;
            if (!Cache.TryGetValue(type, out serializer))
            {
                serializer = new System.Xml.Serialization.XmlSerializer(type);
                Cache.Add(type, serializer);
            }
            return serializer;
        }

        private class IgnoreXmlDeclarationWriter : XmlTextWriter
        {
            public IgnoreXmlDeclarationWriter(TextWriter w) : base(w) { Formatting = Formatting.Indented; }
            public override void WriteStartDocument()
            {

            }

            private static string CamelCase(string name)
            {
                // I think kzu wrote this
                if (name.Length == 0) return name;
                if (char.IsLower(name[0])) return name;
                if (name.Length == 1) return name.ToLower();
                var letters = name.ToCharArray();
                letters[0] = char.ToLower(letters[0]);
                return new string(letters);
            }

            public override void WriteQualifiedName(string localName, string ns)
            {
                base.WriteQualifiedName(CamelCase(localName), ns);
            }
            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                base.WriteStartAttribute(prefix, CamelCase(localName), ns);
            }
            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                base.WriteStartElement(prefix, CamelCase(localName), ns);
            }
        }

        private class IgnoreNamespacesReader : XmlTextReader
        {
            public IgnoreNamespacesReader(TextReader reader)
                : base(reader)
            {

            }
            public override string NamespaceURI
            {
                get { return string.Empty; }
            }
        }
        
        public void Dispose()
        {

        }
    }
}