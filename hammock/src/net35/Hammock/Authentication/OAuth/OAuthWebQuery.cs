using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthWebQuery : WebQuery
    {
        public virtual string Realm { get; set; }
        public virtual OAuthParameterHandling ParameterHandling { get; private set; }
        private bool _recalculate;

        public OAuthWebQuery(OAuthWebQueryInfo info, bool enableTrace) : base(info, enableTrace)
        {
            Initialize(info);
        }

        private void Initialize(OAuthWebQueryInfo info)
        {
            Method = info.WebMethod;
            ParameterHandling = info.ParameterHandling;
            if(info.FirstUse)
            {
                _recalculate = false;
            }
        }

        protected override Func<string, string> BeforeBuildPostOrPutFormWebRequest()
        {
            return post =>
                       {
                           post = AppendParameters(post);
                           
                           return post;
                       };
        }

        protected override byte[] BuildPostOrPutContent(WebRequest request, string post)
        {
            var content = PostProcessPostParameters(request, post.AsUri());
#if TRACE
            Trace.WriteLineIf(TraceEnabled, string.Concat("\r\n", content));            
#endif
			return content;
        }
        
        protected override Func<string, string> BeforeBuildGetDeleteHeadOptionsWebRequest()
        {
            return GetOAuthUrl;
        }

        protected override Func<string, string> BeforeBuildPostOrPutEntityWebRequest()
        {
            return GetOAuthUrl;
        }

        private string GetOAuthUrl(string url)
        {
            // [DC]: Prior to this call, there should be no parameter encoding
            url = PreProcessPostParameters(url);
            
            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    url = AppendParameters(url, true /* escape */, true /* skipOAuth */);
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    url = GetAddressWithOAuthParameters(new Uri(url));
                    break;
            }

            return url;
        }

        protected override string AppendParameters(string url)
        {
            return AppendParameters(url, true /* escape */, false /* skipOAuth */);
        }

        protected virtual string AppendParameters(string url, bool escape, bool skipOAuth)
        {
            var parameters = 0;
            foreach (var parameter in Parameters.Where(
                parameter => !(parameter is HttpPostParameter) || Method == WebMethod.Post
                             ))
            {
                if (skipOAuth && parameter.Name.StartsWith("oauth_"))
                {
                    continue;
                }

                var value = escape
                                ? OAuthTools.UrlEncodeStrict(parameter.Value)
                                : parameter.Value;

                // GET parameters in URL
                url = url.Then(parameters > 0 || url.Contains("?") ? "&" : "?");
                url = url.Then("{0}={1}".FormatWith(parameter.Name, value));
                parameters++;
            }

            return url;
        }

        private string GetAddressWithOAuthParameters(Uri address)
        {
            var sb = new StringBuilder("?");
            var parameters = 0;
            foreach (var parameter in Parameters)
            {
                if (parameter.Name.IsNullOrBlank() || parameter.Value.IsNullOrBlank())
                {
                    continue;
                }

                parameters++;
                var format = parameters < Parameters.Count ? "{0}={1}&" : "{0}={1}";
                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            return address + sb.ToString();
        }

        private byte[] PostProcessPostParameters(WebRequest request, Uri uri)
        {
            var body = "";
            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    SetAuthorizationHeader(request, "Authorization");                    
#if SILVERLIGHT
                    var postParameters = new WebParameterCollection(uri.Query.ParseQueryString());
#else
                    var postParameters = new WebParameterCollection(uri.Query.ParseQueryString());
#endif
                    // Only use the POST parameters that exist in the body
                    postParameters = new WebParameterCollection(postParameters.Where(p => !p.Name.StartsWith("oauth_")));

                    // Append any leftover values to the POST body
                    var nonAuthParameters = GetPostParametersValue(postParameters, true /* escapeParameters */);
                    if (body.IsNullOrBlank())
                    {
                        body = nonAuthParameters;
                    }
                    else
                    {
                        if (!nonAuthParameters.IsNullOrBlank())
                        {
                            body += "&".Then(nonAuthParameters);
                        }
                    }
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    body = GetPostParametersValue(Parameters, false /* escapeParameters */);
                    break;
            }

            var content = Encoding.UTF8.GetBytes(body);
            return content;
        }

        // Removes POST parameters from query
        private static string PreProcessPostParameters(string url)
        {
            var uri = url.AsUri();
            url = uri.Scheme.Then("://")
#if !SILVERLIGHT
                .Then(uri.Authority);
#else
                .Then(uri.Host);
                if ((uri.Scheme.Equals("http") && uri.Port != 80) ||
                    (uri.Scheme.Equals("https") && uri.Port != 443))
                {
                    url = url.Then(":" + uri.Port);
                }
#endif
            url = url.Then(uri.AbsolutePath);
            return url;
        }

        private static string GetPostParametersValue(ICollection<WebPair> postParameters, bool escapeParameters)
        {
            var body = "";
            var count = 0;
            var parameters = postParameters.Where(p => !p.Name.IsNullOrBlank() &&
                                                       !p.Value.IsNullOrBlank()).ToList();

            foreach (var postParameter in parameters)
            {
                // [DC]: client_auth method does not function when these are escaped
                var name = escapeParameters
                               ? OAuthTools.UrlEncodeStrict(postParameter.Name)
                               : postParameter.Name;
                var value = escapeParameters
                                ? OAuthTools.UrlEncodeStrict(postParameter.Value)
                                : postParameter.Value;

                var token = "{0}={1}".FormatWith(name, value);
                body = body.Then(token);
                count++;
                if (count < postParameters.Count)
                {
                    body = body.Then("&");
                }
            }
            return body;
        }

        protected override void AuthenticateRequest(WebRequest request)
        {
            switch(ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    SetAuthorizationHeader(request, "Authorization");
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    // [DC]: Handled in builder method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetAuthorizationContent()
        {
            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    return BuildAuthorizationHeader();
                case OAuthParameterHandling.UrlOrPostParameters:
                    return GetPostParametersValue(Parameters, false /* escapeParameters */);
                default:
                    return "";
            }
        }
        
        protected override void SetAuthorizationHeader(WebRequest request, string header)
        {
            request.Headers[header] = BuildAuthorizationHeader();
        }

        private string BuildAuthorizationHeader()
        {
            var sb = new StringBuilder("OAuth ");
            if (!Realm.IsNullOrBlank())
            {
                sb.Append("realm=\"{0}\",".FormatWith(OAuthTools.UrlEncodeRelaxed(Realm)));
            }

            Parameters.Sort((l, r) => l.Name.CompareTo(r.Name));

            var parameters = 0;
            var pairs = Parameters.Where(parameter => !parameter.Name.IsNullOrBlank() && !parameter.Value.IsNullOrBlank() && parameter.Name.StartsWith("oauth_")); 
            foreach (var parameter in pairs)
            {
                parameters++;
                var format = parameters < pairs.Count() ? "{0}=\"{1}\"," : "{0}=\"{1}\"";
                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            var authorization = sb.ToString();
            return authorization;
        }
       
#if !SILVERLIGHT
        public override void Request(string url, IEnumerable<HttpPostParameter> parameters, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            base.Request(url, parameters, out exception);
        }

        public override void Request(string url, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            url = RestoreUrlParams(url, Parameters);
            base.Request(url, out exception);
        }

        public override void Request(string url, string key, ICache cache, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            base.Request(url, key, cache, out exception);
        }

        public override void Request(string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            base.Request(url, key, cache, absoluteExpiration, out exception);
        }

        public override void Request(string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            base.Request(url, key, cache, slidingExpiration, out exception);
        }
#endif

#if !WindowsPhone
        public override WebQueryAsyncResult RequestAsync(string url, IEnumerable<HttpPostParameter> parameters, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            return base.RequestAsync(url, parameters, userState);
        }

        public override WebQueryAsyncResult RequestAsync(string url, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            url = RestoreUrlParams(url, Parameters);
            return base.RequestAsync(url, userState);
        }

        public override WebQueryAsyncResult RequestAsync(string url, string key, ICache cache, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            return base.RequestAsync(url, key, cache, userState);
        }

        public override WebQueryAsyncResult RequestAsync(string url, string key, ICache cache, DateTime absoluteExpiration, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            return base.RequestAsync(url, key, cache, absoluteExpiration, userState);
        }

        public override WebQueryAsyncResult RequestAsync(string url, string key, ICache cache, TimeSpan slidingExpiration, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            return base.RequestAsync(url, key, cache, slidingExpiration, userState);
        }
#else
        public override void RequestAsync(string url, IEnumerable<HttpPostParameter> parameters, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            base.RequestAsync(url, parameters, userState);
        }

        public override void RequestAsync(string url, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            url = RestoreUrlParams(url, Parameters);
            base.RequestAsync(url, userState);
        }

        public override void RequestAsync(string url, string key, ICache cache, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            base.RequestAsync(url, key, cache, userState);
        }

        public override void RequestAsync(string url, string key, ICache cache, DateTime absoluteExpiration, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            base.RequestAsync(url, key, cache, absoluteExpiration, userState);
        }

        public override void RequestAsync(string url, string key, ICache cache, TimeSpan slidingExpiration, object userState)
        {
            RecalculateProtectedResourceSignature(url);
            base.RequestAsync(url, key, cache, slidingExpiration, userState);
        }
#endif
        
        private string RestoreUrlParams(string url, IEnumerable<WebPair> parameters)
        {
            if (Method != WebMethod.Post && Method != WebMethod.Put)
            {
                var builder = new StringBuilder();
                var first = true;
                foreach (var param in parameters.Where(p => !p.Name.ToLower().StartsWith("oauth_")))
                {
                    builder.Append(first ? "?" : "&");
                    first = false;
                    builder.Append(param.Name).Append('=').Append(param.Value);
                }
                url = url + builder;
            }
            return url;
        }

        private void RecalculateProtectedResourceSignature(string url)
        {
            if(!_recalculate)
            {
                _recalculate = true;
                return; // <-- More efficient for unrecycled queries
            }

            var info = (OAuthWebQueryInfo) Info;

            if(!info.ClientUsername.IsNullOrBlank() || !info.ClientPassword.IsNullOrBlank())
            {
                // Not a protected resource request
                return;
            }

            if(!string.IsNullOrEmpty(info.Verifier))
            {
                // This is an access token request
                return;
            }

            var oauth = new OAuthWorkflow
                            {
                                ConsumerKey = info.ConsumerKey,
                                ConsumerSecret = info.ConsumerSecret,
                                Token = info.Token,
                                TokenSecret = info.TokenSecret,
                                ClientUsername = info.ClientUsername,
                                ClientPassword = info.ClientPassword,
                                SignatureMethod = info.SignatureMethod.FromRequestValue(),
                                ParameterHandling = ParameterHandling,
                                CallbackUrl = info.Callback,
                                Verifier = info.Verifier
                            };

            // [DC]: Add any non-oauth parameters back into the signature hash
            var parameters = new WebParameterCollection();
            var nonAuthParameters = Parameters.Where(p => !p.Name.StartsWith("oauth_"));
            parameters.AddRange(nonAuthParameters);
            
            // [DC]: Don't escape parameters again when calcing the signature
            Info = oauth.BuildProtectedResourceInfo(Method, parameters, url);

            // [DC]: Add any non-oauth parameters back into parameter bag
            Parameters = ParseInfoParameters();
            Parameters.AddRange(nonAuthParameters);
        }
    }
}