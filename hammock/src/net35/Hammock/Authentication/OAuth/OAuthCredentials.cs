using System;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthCredentials : IWebCredentials
    {
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
        public virtual OAuthParameterHandling ParameterHandling { get; set; }
        public virtual OAuthSignatureMethod SignatureMethod { get; set; }
        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
        public virtual OAuthType Type { get; set; }

        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
        public virtual string Verifier { get; set; }
        public virtual string ClientUsername { get; set; }
        public virtual string ClientPassword { get; set; }
        public virtual string CallbackUrl { get; set; }
        public virtual string Version { get; set; }
        public virtual string SessionHandle { get; set; }

        public static RestRequest DelegateWith(RestClient client, RestRequest request)
        {
            if(request == null)
            {
                throw new ArgumentNullException("request");
            }

            if(!request.Method.HasValue)
            {
                throw new ArgumentException("Request must specify a web method.");
            }

            var method = request.Method.Value;
            var credentials = (OAuthCredentials)request.Credentials;
            var url = request.BuildEndpoint(client).ToString();
            var workflow = new OAuthWorkflow(credentials);
            var uri = new Uri(client.Authority);
            var realm = uri.Host;
            var enableTrace = client.TraceEnabled || request.TraceEnabled;

            var info = workflow.BuildProtectedResourceInfo(method, request.GetAllHeaders(), url);
            var query = credentials.GetQueryFor(url, request, info, method, enableTrace);
            ((OAuthWebQuery) query).Realm = realm;
            var auth = query.GetAuthorizationContent();

            var echo = new RestRequest();
            echo.AddHeader("X-Auth-Service-Provider", url);
            echo.AddHeader("X-Verify-Credentials-Authorization", auth);
            return echo;
        }
        
        public static OAuthCredentials ForRequestToken(string consumerKey, string consumerSecret)
        {
            var credentials = new OAuthCredentials
                                  {
                                      Type = OAuthType.RequestToken,
                                      ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                      SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                      SignatureTreatment = OAuthSignatureTreatment.Escaped,
                                      ConsumerKey = consumerKey,
                                      ConsumerSecret = consumerSecret
                                  };
            return credentials;
        }

        public static OAuthCredentials ForRequestToken(string consumerKey, string consumerSecret, string callbackUrl)
        {
            var credentials = ForRequestToken(consumerKey, consumerSecret);
            credentials.CallbackUrl = callbackUrl;
            return credentials;
        }

        public static OAuthCredentials ForAccessToken(string consumerKey, string consumerSecret, string requestToken, string requestTokenSecret)
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.AccessToken,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = requestToken,
                TokenSecret = requestTokenSecret
            };
            return credentials;
        }

        public static OAuthCredentials ForAccessToken(string consumerKey, string consumerSecret, string requestToken, string requestTokenSecret, string verifier)
        {
            var credentials = ForAccessToken(consumerKey, consumerSecret, requestToken, requestTokenSecret);
            credentials.Verifier = verifier;
            return credentials;
        }

        public static OAuthCredentials ForAccessTokenRefresh(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string sessionHandle)
        {
            var credentials = ForAccessToken(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            credentials.SessionHandle = sessionHandle;
            return credentials;
        }

        public static OAuthCredentials ForAccessTokenRefresh(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string sessionHandle, string verifier)
        {
            var credentials = ForAccessToken(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            credentials.SessionHandle = sessionHandle;
            credentials.Verifier = verifier;
            return credentials;
        }

        public static OAuthCredentials ForClientAuthentication(string consumerKey, string consumerSecret, string username, string password)
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ClientAuthentication,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                ClientUsername = username,
                ClientPassword = password
            };

            return credentials;
        }

        public static OAuthCredentials ForProtectedResource(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                SignatureTreatment = OAuthSignatureTreatment.Escaped,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = accessToken,
                TokenSecret = accessTokenSecret
            };
            return credentials;
        }
       
        public virtual WebQuery GetQueryFor(string url, 
                                            WebParameterCollection parameters, 
                                            IWebQueryInfo info, 
                                            WebMethod method,
                                            bool enableTrace)
        {
            OAuthWebQueryInfo oauth;

            var workflow = new OAuthWorkflow
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                ParameterHandling = ParameterHandling,
                SignatureMethod = SignatureMethod,
                SignatureTreatment = SignatureTreatment,
                CallbackUrl = CallbackUrl,
                ClientPassword = ClientPassword,
                ClientUsername = ClientUsername,
                Verifier = Verifier,
                Token = Token,
                TokenSecret = TokenSecret,
                Version = Version ?? "1.0",
                SessionHandle = SessionHandle
            };

            switch (Type)
            {
                case OAuthType.RequestToken:
                    workflow.RequestTokenUrl = url;
                    oauth = workflow.BuildRequestTokenInfo(method, parameters);
                    break;
                case OAuthType.AccessToken:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ClientAuthentication:
                    method = WebMethod.Post;
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildClientAuthAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ProtectedResource:
                    oauth = workflow.BuildProtectedResourceInfo(method, parameters, url);
                    oauth.FirstUse = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OAuthWebQuery(oauth, enableTrace);
        }

        public virtual WebQuery GetQueryFor(string url, RestBase request, IWebQueryInfo info, WebMethod method, bool enableTrace)
        {
            var query = GetQueryFor(url, request.Parameters, info, method, enableTrace);
            request.Method = method;
            return query;
        }
    }
}


