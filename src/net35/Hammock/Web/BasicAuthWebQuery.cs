using System;
using System.Net;
using Hammock.Extensions;

namespace Hammock.Web
{
    /// <summary>
    /// A web query engine for making requests that use basic HTTP authorization.
    /// </summary>
    public class BasicAuthWebQuery : WebQuery
    {
        private readonly string _password;
        private readonly string _username;

        public BasicAuthWebQuery(IWebQueryInfo info, string username, string password, bool enableTrace) : this(info, enableTrace)
        {
            _username = username;
            _password = password;
        }

        public BasicAuthWebQuery(IWebQueryInfo info, bool enableTrace) : base(info, enableTrace)
        {

        }

        public bool HasAuth
        {
            get
            {
                return
                    (!_username.IsNullOrBlank()
                     && !String.IsNullOrEmpty(_password));
            }
        }

        protected override void SetAuthorizationHeader(WebRequest request, string header)
        {
            if (!HasAuth)
            {
                return;
            }

            var credentials = GetAuthorizationHeader();
            request.Headers[header] = credentials;
        }

        private string GetAuthorizationHeader()
        {
            return WebExtensions.ToBasicAuthorizationHeader(_username, _password);
        }

        protected override void AuthenticateRequest(WebRequest request)
        {
            SetAuthorizationHeader(request, "Authorization");
        }

        public override string GetAuthorizationContent()
        {
            return GetAuthorizationHeader();
        }
    }
}