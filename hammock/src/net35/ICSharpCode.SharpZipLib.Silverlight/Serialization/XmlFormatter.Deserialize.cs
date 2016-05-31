using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    /// <summary>
    /// Code originally authored by Rockford Lhotka:
    /// http://www.lhotka.net/weblog/SilverlightSerialization.aspx,
    /// presented here with minor naming and code changes.
    /// </summary>
    public sealed partial class XmlFormatter
    {
        private readonly Dictionary<ISerializable, SerializationInfo> _serializationReferences =
            new Dictionary<ISerializable, SerializationInfo>();

        public void Serialize(Stream serializationStream, object graph)
        {
            var writer = XmlWriter.Create(serializationStream);
            Serialize(writer, graph);
            if (writer != null)
            {
                writer.Flush();
            }
        }

        public void Serialize(TextWriter textWriter, object graph)
        {
            var writer = XmlWriter.Create(textWriter);
            Serialize(writer, graph);
            if (writer != null)
            {
                writer.Flush();
            }
        }

        public void Serialize(XmlWriter writer, object graph)
        {
            _serializationReferences.Clear();

            var document = new XDocument();
            SerializeObject(graph);
            var root = new XElement("g");
            foreach (var item in _serializationReferences)
            {
                root.Add(item.Value.ToXElement());
            }
            document.Add(root);
            document.Save(writer);
        }

        internal SerializationInfo SerializeObject(object obj)
        {
            var thisType = obj.GetType();
            if (!IsSerializable(thisType))
            {
                throw new InvalidOperationException("Object not serializable");
            }
            var mobile = obj as ISerializable;
            if (mobile == null)
            {
                throw new InvalidOperationException(
                    string.Format("Type {0} must implement ISerializable",
                                  thisType.Name));
            }

            SerializationInfo info;
            if (!_serializationReferences.TryGetValue(mobile, out info))
            {
                info = new SerializationInfo(_serializationReferences.Count + 1);
                _serializationReferences.Add(mobile, info);
                mobile.Serialize(info, this);
            }
            return info;
        }

        private static bool IsSerializable(ICustomAttributeProvider objectType)
        {
            var a = objectType.GetCustomAttributes(typeof (SerializableAttribute), false);
            return a.Length > 0;
        }
    }
}