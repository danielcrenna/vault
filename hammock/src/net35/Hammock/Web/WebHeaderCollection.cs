using System.Collections.Generic;
#if !SILVERLIGHT
using System.Collections.Specialized;
#else
using Hammock.Silverlight.Compat;
#endif
using System.Linq;

namespace Hammock.Web
{
    public class WebHeaderCollection : WebPairCollection
    {
        public WebHeaderCollection(NameValueCollection collection) : base(collection)
        {
        }

        public WebHeaderCollection(IEnumerable<WebPair> parameters) : base(parameters)
        {
        }

        public WebHeaderCollection()
        {
        }

        public WebHeaderCollection(IDictionary<string, string> collection) : base(collection)
        {
        }

        public WebHeaderCollection(int capacity) : base(capacity)
        {
        }

        public override WebPair this[string name]
        {
            // Headers can be non-unique
            get { return this.First(p => p.Name.Equals(name)); }
        }

        public IEnumerable<string> AllKeys
        {
            get
            {
                return Names;
            }
        }
    }
}