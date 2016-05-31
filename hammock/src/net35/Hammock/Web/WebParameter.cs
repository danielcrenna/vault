#if !Smartphone && !NETCF
using System.Diagnostics;
#endif
using System;
namespace Hammock.Web
{
#if !Smartphone && !NETCF
    [DebuggerDisplay("{Name}:{Value}")]
#endif
#if !SILVERLIGHT
  [Serializable]
#endif
    public class WebParameter : WebPair
    {
        public WebParameter(string name, string value) : base(name, value)
        {

        }
    }
}