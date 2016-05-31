using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ICSharpCode.SharpZipLib.Silverlight.Serialization
{
    /// <summary>
    /// Code originally authored by Rockford Lhotka:
    /// http://www.lhotka.net/weblog/SilverlightSerialization.aspx,
    /// presented here with minor naming and code changes.
    /// </summary>
    public class SerializationInfo
    {
        private class ValueEntry
        {
            public string Name { get; private set; }
            public object Value { get; private set; }

            public ValueEntry(string name, object value)
            {
                Name = name;
                Value = value;
            }
        }

        private readonly Dictionary<string, ValueEntry> _values = new Dictionary<string, ValueEntry>();

        internal SerializationInfo(int referenceId)
        {
            ReferenceId = referenceId;
        }

        internal int ReferenceId { get; private set; }
        
        public string TypeName { get; set; }

        public void AddValue(string name, object value)
        {
            _values.Add(name, new ValueEntry(name, value));
        }

        public object GetValue(string name)
        {
            ValueEntry result;
            return _values.TryGetValue(name, out result) ? result.Value : null;
        }

        internal XElement ToXElement()
        {
            var root = new XElement("o");
            root.Add(new XAttribute("i", ReferenceId));
            root.Add(new XAttribute("t", TypeName));
            
            foreach (var item in _values)
            {
                var info = item.Value.Value as SerializationInfo;
                if (info == null)
                {
                    var list = item.Value.Value as List<SerializationInfo>;
                    if (list == null)
                    {
                        if (item.Value.Value != null)
                            root.Add(new XElement("f",
                                                  new XAttribute("n", item.Value.Name),
                                                  new XAttribute("v", item.Value.Value)));
                    }
                    else
                    {
                        var listElement = new XElement("l",
                                                       new XAttribute("n", item.Value.Name));
                        foreach (var listItem in list)
                            listElement.Add(new XElement("r",
                                                         new XAttribute("i", listItem.ReferenceId)));
                        root.Add(listElement);
                    }
                }
                else
                    root.Add(new XElement("r",
                                          new XAttribute("n", item.Value.Name),
                                          new XAttribute("i", info.ReferenceId)));
            }
            return root;
        }

        internal SerializationInfo(XElement data)
        {
            ReferenceId = Convert.ToInt32(data.Attribute("i").Value);
            if (data.Name == "o")
            {
                TypeName = data.Attribute("t").Value;
            }
        }

        internal void Deserialize(XElement data, XmlFormatter formatter)
        {
            foreach (var item in data.Elements())
            {
                if (item.Name == "f")
                {
                    var entry = new ValueEntry(item.Attribute("n").Value, item.Attribute("v").Value);
                    _values.Add(entry.Name, entry);
                }
                else if (item.Name == "l")
                {
                    var listItems = item.Elements().Select(content => new SerializationInfo(content)).ToList();

                    var entry = new ValueEntry(item.Attribute("n").Value, listItems);
                    _values.Add(entry.Name, entry);
                }
                else
                {
                    var referenceId = Convert.ToInt32(item.Attribute("i").Value);
                    var entry = new ValueEntry(
                        item.Attribute("n").Value, 
                        new SerializationInfo(referenceId));
                    _values.Add(entry.Name, entry);
                }
            }
        }
    }
}