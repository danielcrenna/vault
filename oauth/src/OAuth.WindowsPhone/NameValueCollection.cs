using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Specialized
{
    public class NameValueCollection : List<KeyValuePair<string, string>>
    {
        public new string this[int index]
        {
            get
            {
                return base[index].Value;
            }
        }

        public string this[string name]
        {
            get
            {
                return this.SingleOrDefault(kv => kv.Key.Equals(name)).Value;
            }
        }

        public NameValueCollection()
        {

        }

        public NameValueCollection(int capacity) : base(capacity)
        {
            
        }

        public void Add(string name, string value)
        {
            Add(new KeyValuePair<string, string>(name, value));
        }

        public IEnumerable<string> AllKeys
        {
            get
            {
                return this.Select(pair => pair.Key);
            }
        }
    }
}
