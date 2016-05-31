using System;
using Hammock.Extensions;
using Hammock.Web;

namespace Hammock.Authentication.Basic
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class BasicAuthCredentials : IWebCredentials
    {
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }

        public WebQuery GetQueryFor(string url, RestBase request, IWebQueryInfo info, WebMethod method, bool enableTrace)
        {
            return GetQueryForImpl(info, enableTrace);
        }

        public WebQuery GetQueryFor(string url, WebParameterCollection parameters, IWebQueryInfo info, WebMethod method, bool enableTrace)
        {
            return GetQueryForImpl(info, enableTrace);
        }

        private WebQuery GetQueryForImpl(IWebQueryInfo info, bool enableTrace)
        {
            return HasAuth
                       ? new BasicAuthWebQuery(info, Username, Password, enableTrace)
                       : new BasicAuthWebQuery(info, enableTrace);
        }

        public virtual bool HasAuth
        {
            get
            {
                return !Username.IsNullOrBlank() && !Password.IsNullOrBlank();
            }
        }
    }
}