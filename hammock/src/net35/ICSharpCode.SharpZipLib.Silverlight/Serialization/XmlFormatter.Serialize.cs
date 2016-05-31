using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly Dictionary<int, ISerializable> _deserializationReferences =
            new Dictionary<int, ISerializable>();

        public object Deserialize(Stream serializationStream)
        {
            var reader = XmlReader.Create(serializationStream);
            return Deserialize(reader);
        }

        public object Deserialize(TextReader textReader)
        {
            var reader = XmlReader.Create(textReader);
            return Deserialize(reader);
        }

        public object Deserialize(XmlReader reader)
        {
            var doc = XDocument.Load(reader);
            var root = (XElement) doc.FirstNode;

            _deserializationReferences.Clear();

            var objects = from e in root.Elements()
                          where e.Name == "o"
                          select e;

            var infos = new Dictionary<int, SerializationInfo>();

            foreach (var item in objects)
            {
                var info = new SerializationInfo(item);
                infos.Add(info.ReferenceId, info);
                var objType = Type.GetType(info.TypeName);
                var mobile = Activator.CreateInstance(objType) as ISerializable;
                _deserializationReferences.Add(info.ReferenceId, mobile);
            }

            foreach (var item in objects)
            {
                if (item == null)
                {
                    continue;
                }

                var referenceId = Convert.ToInt32(item.Attribute("i").Value);
                var info = infos[referenceId];
                info.Deserialize(item, this);
            }

            foreach (var info in infos)
            {
                GetObject(info.Value.ReferenceId).Deserialize(info.Value, this);
            }

            return _deserializationReferences[1];
        }

        internal ISerializable GetObject(int referenceId)
        {
            return _deserializationReferences[referenceId];
        }
    }
}