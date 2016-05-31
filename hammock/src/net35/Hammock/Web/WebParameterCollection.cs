using System.Collections.Generic;
#if !SILVERLIGHT
using System.Collections.Specialized;
#else
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Web
{
    public class WebParameterCollection : WebPairCollection
    {
        public WebParameterCollection(IEnumerable<WebPair> parameters)
            : base(parameters)
        {

        }

        public WebParameterCollection(NameValueCollection collection) : base(collection)
        {
        }

        public WebParameterCollection()
        {
        }

        public WebParameterCollection(int capacity) : base(capacity)
        {
        }

        public WebParameterCollection(IDictionary<string, string> collection) : base(collection)
        {

        }

        public override void Add(string name, string value)
        {
            var parameter = new WebParameter(name, value);
            base.Add(parameter);
        }
    }
}