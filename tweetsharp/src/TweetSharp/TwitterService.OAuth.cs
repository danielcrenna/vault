using System;
using System.Globalization;
using System.Net;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Web;

#if !SILVERLIGHT && !WINRT
using System.Compat.Web;
#endif

#if SILVERLIGHT && !WINDOWS_PHONE
using HttpUtility = TweetSharp.Silverlight.HttpUtility;
#endif

#if WINDOWS_PHONE
using HttpUtility = System.Web.HttpUtility;
#endif

namespace TweetSharp
{
    public partial class TwitterService
		{
#if !SILVERLIGHT && !WINRT
        [Serializable]
#endif
			private class FunctionArguments
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string Token { get; set; }
            public string TokenSecret { get; set; }
            public string Verifier { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private readonly Func<FunctionArguments, RestRequest> _requestTokenQuery
            = args =>
            {
                var request = new RestRequest
                {
                    Credentials = new OAuthCredentials
                    {
                        ConsumerKey = args.ConsumerKey,
                        ConsumerSecret = args.ConsumerSecret,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        Type = OAuthType.RequestToken
                    },
                    Method = WebMethod.Get,
                    Path = "/oauth/request_token"
                };
                return request;
            };

        private readonly Func<FunctionArguments, RestRequest> _accessTokenQuery
            = args =>
            {
                var request = new RestRequest
                {
                    Credentials = new OAuthCredentials
                    {
                        ConsumerKey = args.ConsumerKey,
                        ConsumerSecret = args.ConsumerSecret,
                        Token = args.Token,
                        TokenSecret = args.TokenSecret,
                        Verifier = args.Verifier,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        Type = OAuthType.AccessToken
                    },
                    Method = WebMethod.Post,
                    Path = "/oauth/access_token"
                };
                return request;
            };

        private readonly Func<FunctionArguments, RestRequest> _protectedResourceQuery
            = args =>
            {
                var request = new RestRequest
                {
                    Credentials = new OAuthCredentials
                    {
                        Type = OAuthType.ProtectedResource,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = args.ConsumerKey,
                        ConsumerSecret = args.ConsumerSecret,
                        Token = args.Token,
                        TokenSecret = args.TokenSecret,
                    }
                };
                return request;
            };

        private readonly Func<FunctionArguments, RestRequest> _xAuthQuery
            = args =>
            {
                var request = new RestRequest
                {
                    Credentials = new OAuthCredentials
                    {
                        Type = OAuthType.ClientAuthentication,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = args.ConsumerKey,
                        ConsumerSecret = args.ConsumerSecret,
                        ClientUsername = args.Username,
                        ClientPassword = args.Password
                    },
                    Method = WebMethod.Post,
                    Path = "/oauth/access_token"
                };
                return request;
            };

        private readonly RestClient _oauth;
    	private const string AuthorizeUrl = Globals.Authority + "/oauth/authorize";
    	private const string AuthenticateUrl = Globals.Authority + "/oauth/authenticate";

        public virtual void AuthenticateWith(string token, string tokenSecret)
        {
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public virtual void AuthenticateWith(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public virtual Uri GetAuthorizationUri(OAuthRequestToken oauth)
        {
			return new Uri(AuthorizeUrl + "?oauth_token=" + oauth.Token);
        }

        public virtual Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback)
        {
			return new Uri(AuthorizeUrl + "?oauth_token=" + oauth.Token + "&oauth_callback=" + callback);
        }

		public virtual Uri GetAuthorizationUri(OAuthRequestToken oauth, CultureInfo culture)
		{
			var languageCode = culture.TwoLetterISOLanguageName;
			return new Uri(AuthorizeUrl + "?oauth_token=" + oauth.Token + "&lang=" + languageCode);
		}

		public virtual Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback, CultureInfo culture)
		{
			var languageCode = culture.TwoLetterISOLanguageName;
			return new Uri(AuthorizeUrl + "?oauth_token=" + oauth.Token + "&oauth_callback=" + callback + "&lang=" + languageCode);
		}

        public virtual Uri GetAuthenticationUrl(OAuthRequestToken oauth)
        {
			return new Uri(AuthenticateUrl + "?oauth_token=" + oauth.Token);
        }

        public virtual Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback)
        {
            return new Uri(AuthenticateUrl +"?oauth_token=" + oauth.Token + "&oauth_callback=" + callback);
        }

		public virtual Uri GetAuthenticationUrl(OAuthRequestToken oauth, CultureInfo culture)
		{
			var languageCode = culture.TwoLetterISOLanguageName;
			return new Uri(AuthenticateUrl +"?oauth_token=" + oauth.Token + "&lang=" + languageCode);
		}

		public virtual Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback, CultureInfo culture)
		{
			var languageCode = culture.TwoLetterISOLanguageName;
			return new Uri(AuthenticateUrl +"?oauth_token=" + oauth.Token + "&oauth_callback=" + callback + "&lang=" + languageCode);
		}

#if !SILVERLIGHT && !WINRT

		public virtual OAuthRequestToken GetRequestToken(string callback)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret
            };

            var request = _requestTokenQuery.Invoke(args);
            if(!callback.IsNullOrBlank())
            {
                request.AddParameter("oauth_callback", callback);
            }

            var response = _oauth.Request(request);

            SetResponse(response);

            var query = HttpUtility.ParseQueryString(response.Content);
            var oauth = new OAuthRequestToken
            {
                Token = query["oauth_token"] ?? "?",
                TokenSecret = query["oauth_token_secret"] ?? "?",
                OAuthCallbackConfirmed = Convert.ToBoolean(query["oauth_callback_confirmed"] ?? "false")
            };

            return oauth;
        }

        public virtual OAuthRequestToken GetRequestToken()
        {
            return GetRequestToken(callback: null);
        }

				public virtual OAuthAccessToken GetAccessTokenWithXAuth(string username, string password)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Username = username,
                Password = password
            };

            var request = _xAuthQuery.Invoke(args);
            var response = _oauth.Request(request);

            SetResponse(response);

            var query = HttpUtility.ParseQueryString(response.Content);
            var accessToken = new OAuthAccessToken
            {
                Token = query["oauth_token"] ?? "?",
                TokenSecret = query["oauth_token_secret"] ?? "?",
                UserId = Convert.ToInt64(query["user_id"] ?? "0"),
                ScreenName = query["screen_name"] ?? "?"
            };

            return accessToken;
        }

        public virtual OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken)
        {
            return GetAccessToken(requestToken, verifier: null);
        }

        public virtual OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken, string verifier)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Token = requestToken.Token,
                TokenSecret = requestToken.TokenSecret,
                Verifier = verifier
            };

            var request = _accessTokenQuery.Invoke(args);
            var response = _oauth.Request(request);

            SetResponse(response);

            var query = HttpUtility.ParseQueryString(response.Content);
            var accessToken = new OAuthAccessToken
                        {
                            Token = query["oauth_token"] ?? "?",
                            TokenSecret = query["oauth_token_secret"] ?? "?",
                            UserId = Convert.ToInt64(query["user_id"] ?? "0"),
                            ScreenName = query["screen_name"] ?? "?"
                        };

            return accessToken;
        }

				public virtual string GetEchoRequest(string url)
				{
					var client = new RestClient { Authority = "" };
					var request = PrepareEchoRequest();
					request.Path = url;
					var response = client.Request(request);
					return response.Content ?? "";
				}

#endif

        public virtual void GetRequestToken(string callback, Action<OAuthRequestToken, TwitterResponse> action)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret
            };

            var request = _requestTokenQuery.Invoke(args);
            if (!callback.IsNullOrBlank())
            {
                request.AddParameter("oauth_callback", callback);
            }

            _oauth.BeginRequest(request,
                (req, resp, state) =>
                {
                    Exception exception;
                    var entity = TryAsyncResponse(() =>
                    {
                        if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                        {
                            return null;
                        }
											
#if WINRT
												var query = new Windows.Foundation.WwwFormUrlDecoder(resp.Content);
												var requestToken = new OAuthRequestToken
												{
													Token = query.GetFirstValueByName("oauth_token") ?? "?",
													TokenSecret = query.GetFirstValueByName("oauth_token_secret") ?? "?",
													OAuthCallbackConfirmed = Convert.ToBoolean(query.GetFirstValueByName("oauth_callback_confirmed") ?? "false")
												};
#else
                        var query = HttpUtility.ParseQueryString(resp.Content);
                        var requestToken = new OAuthRequestToken
                        {
                            Token = query["oauth_token"] ?? "?",
                            TokenSecret = query["oauth_token_secret"] ?? "?",
                            OAuthCallbackConfirmed = Convert.ToBoolean(query["oauth_callback_confirmed"] ?? "false")
                        };
#endif
                        return requestToken;
                    }, 
                    out exception);

                    action(entity, new TwitterResponse(resp, exception));
                });
        }

        public virtual void GetRequestToken(Action<OAuthRequestToken, TwitterResponse> action)
        {
            GetRequestToken("", action);
        }

        public virtual void GetAccessTokenWithXAuth(string username, string password, Action<OAuthAccessToken, TwitterResponse> action)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Username = username,
                Password = password
            };

            var request = _xAuthQuery.Invoke(args);

            _oauth.BeginRequest(request,
                                (req, resp, state) =>
                                    {
                                        Exception exception;
                                        var entity = TryAsyncResponse(() =>
                                        {
                                            if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                                            {
                                                return null;
                                            }

#if WINRT
																						var query = new Windows.Foundation.WwwFormUrlDecoder(resp.Content);
                                            var accessToken = new OAuthAccessToken
                                            {
                                                Token = query.GetFirstValueByName("oauth_token") ?? "?",
                                                TokenSecret = query.GetFirstValueByName("oauth_token_secret") ?? "?",
                                                UserId = Convert.ToInt64(query.GetFirstValueByName("user_id") ?? "0"),
                                                ScreenName = query.GetFirstValueByName("screen_name") ?? "?"
                                            };
#else
																					  var query = HttpUtility.ParseQueryString(resp.Content);
                                            var accessToken = new OAuthAccessToken
                                            {
                                                Token = query["oauth_token"] ?? "?",
                                                TokenSecret = query["oauth_token_secret"] ?? "?",
                                                UserId = Convert.ToInt64(query["user_id"] ?? "0"),
                                                ScreenName = query["screen_name"] ?? "?"
                                            };
#endif
																						return accessToken;
                                        },
                                        out exception);

                                        action(entity, new TwitterResponse(resp, exception));
                                    }
                );
        }

        public virtual void GetAccessToken(OAuthRequestToken requestToken, Action<OAuthAccessToken, TwitterResponse> action)
        {
            GetAccessToken(requestToken, null, action);
        }

        public virtual void GetAccessToken(OAuthRequestToken requestToken, string verifier, Action<OAuthAccessToken, TwitterResponse> action)
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Token = requestToken.Token,
                TokenSecret = requestToken.TokenSecret,
                Verifier = verifier
            };

            var request = _accessTokenQuery.Invoke(args);

           _oauth.BeginRequest(request, (req, resp, state) =>
                               {
                                   Exception exception;
                                   var entity = TryAsyncResponse(() =>
                                   {
                                        if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                                        {
                                            return null;
                                        }

#if WINRT
																				var query = new Windows.Foundation.WwwFormUrlDecoder(resp.Content);
																				var accessToken = new OAuthAccessToken
																				{
																					Token = query.GetFirstValueByName("oauth_token") ?? "?",
																					TokenSecret = query.GetFirstValueByName("oauth_token_secret") ?? "?",
																					UserId = Convert.ToInt64(query.GetFirstValueByName("user_id") ?? "0"),
																					ScreenName = query.GetFirstValueByName("screen_name") ?? "?"
																				};
#else
                                       var query = HttpUtility.ParseQueryString(resp.Content);
                                       var accessToken = new OAuthAccessToken
                                       {
                                           Token = query["oauth_token"] ?? "?",
                                           TokenSecret = query["oauth_token_secret"] ?? "?",
                                           UserId = Convert.ToInt64(query["user_id"] ?? "0"),
                                           ScreenName = query["screen_name"] ?? "?"
                                       };
#endif
                                       return accessToken;
                                   },
                                   out exception);

                                   action(entity, new TwitterResponse(resp, exception));
                               }
               );
        }

        public virtual void GetEchoRequest(string url, Action<string, TwitterResponse> action)
        {
            var client = new RestClient { Authority = "" };
            var request = PrepareEchoRequest();
            request.Path = url;
            
            client.BeginRequest(request,
                               (req, resp, state) =>
                               {
                                   Exception exception;
                                   var entity = TryAsyncResponse(() =>
                                   {
                                       if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                                       {
                                           return "";
                                       }

                                       var content = resp.Content ?? "";
                                       return content;
                                   },
                                   out exception);
                                   action.Invoke(entity, new TwitterResponse(resp));
                               }
               );
        }

        public virtual RestRequest PrepareEchoRequest(string realm = "http://api.twitter.com")
        {
            var args = new FunctionArguments
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                Token = _token,
                TokenSecret = _tokenSecret
            };

            var request = _protectedResourceQuery.Invoke(args);
            request.Method = WebMethod.Get;
            request.Path = string.Concat("account/verify_credentials", FormatAsString);

            var credentials = (OAuthCredentials)request.Credentials;
            var url = request.BuildEndpoint(_client);
            var workflow = new OAuthWorkflow(credentials);

            var method = request.Method.HasValue ? request.Method.Value : WebMethod.Get;
            var info = workflow.BuildProtectedResourceInfo(method, request.GetAllHeaders(), url);
            var query = credentials.GetQueryFor(url, request, info, method, TraceEnabled);

            ((OAuthWebQuery)query).Realm = realm;
            var auth = query.GetAuthorizationContent();

            var echo = new RestRequest();
            echo.AddHeader("X-Auth-Service-Provider", url);
            echo.AddHeader("X-Verify-Credentials-Authorization", auth);
            return echo;
        }

    }
}
