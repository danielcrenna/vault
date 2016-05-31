using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Hammock;
using Hammock.Serialization;
using Hammock.Web;

#if PLATFORM_SUPPORTS_ASYNC_AWAIT
using System.Threading.Tasks;
#endif

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace TweetSharp
{
    /// <summary>
    /// Defines a contract for a <see cref="TwitterService" /> implementation.
    /// </summary>
    /// <seealso href="http://dev.twitter.com/doc" />
    public partial class TwitterService
    {
        private readonly RestClient _client;
				private readonly RestClient _uploadMediaClient;

        public bool TraceEnabled { get; set; }
        public string Proxy { get; set; }
        public bool IncludeEntities { get; set; }
        public bool IncludeRetweets { get; set; }

        public string UserAgent
        {
            get { return _client.UserAgent; }
            set
            {
                _client.UserAgent = value;
                _publicStreamsClient.UserAgent = value;
                _userStreamsClient.UserAgent = value;
                _oauth.UserAgent = value;
            }
        }

        public IDeserializer Deserializer
        {
            get { return _client.Deserializer; }
            set { 
                _client.Deserializer = value;
                _userStreamsClient.Deserializer = value;
                _publicStreamsClient.Deserializer = value;
            }
        }

        public ISerializer Serializer
        {
            get { return _client.Serializer; }
            set { _client.Serializer = value;
                _userStreamsClient.Serializer = value;
                _publicStreamsClient.Serializer = value;
            }
        }
        
        private string _consumerKey;
        private string _consumerSecret;
        private string _token;
        private string _tokenSecret;

#if !WINDOWS_PHONE
        private void SetResponse(RestResponseBase response)
        {
            Response = new TwitterResponse(response);
        }
#endif

 #if !SILVERLIGHT && !WINRT
        static TwitterService()
        {
            ServicePointManager.Expect100Continue = false;
        }
#endif

#if !WINDOWS_PHONE
        public virtual TwitterResponse Response { get; private set; }
#endif

        public TwitterService(TwitterClientInfo info) : this()
        {
            _consumerKey = info.ConsumerKey;
            _consumerSecret = info.ConsumerSecret;
            IncludeEntities = info.IncludeEntities;

            _info = info;
        }
        
        public TwitterService(string consumerKey, string consumerSecret) : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public TwitterService(string consumerKey, string consumerSecret, ISerializer serializer, IDeserializer deserializer)
            : this(serializer, deserializer)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }
        
        public TwitterService(string consumerKey, string consumerSecret, string proxy) : this(proxy: proxy)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public TwitterService(string consumerKey, string consumerSecret, string token, string tokenSecret) : this()
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public TwitterService(string consumerKey, string consumerSecret, string token, string tokenSecret, ISerializer serializer, IDeserializer deserializer)
             : this(serializer, deserializer)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public TwitterService(ISerializer serializer = null, IDeserializer deserializer = null, string proxy = null)
        {
            Proxy = proxy;
            FormatAsString = ".json";
            
            var jsonSerializer = new JsonSerializer();
            const string userAgent = "TweetSharp";

            _oauth = new RestClient
            {
                Authority = Globals.Authority,
                Proxy = Proxy,
                UserAgent = userAgent,
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterErrors),
#if SILVERLIGHT 
                HasElevatedPermissions = true
#endif
            };

            _client = new RestClient
            {
                Authority = Globals.Authority,
                QueryHandling = QueryHandling.AppendToParameters,
                VersionPath = "1.1",
                Serializer = serializer ?? jsonSerializer,
                Deserializer = deserializer ?? jsonSerializer,
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterErrors),
                UserAgent = userAgent,
                Proxy = Proxy,
#if !SILVERLIGHT && !WINRT
                FollowRedirects = true,
#endif
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
            };

						_uploadMediaClient = new RestClient
						{
							Authority = Globals.MediaUploadAuthority,
							QueryHandling = QueryHandling.AppendToParameters,
							VersionPath = "1.1",
							Serializer = serializer ?? jsonSerializer,
							Deserializer = deserializer ?? jsonSerializer,
							DecompressionMethods = DecompressionMethods.GZip,
							GetErrorResponseEntityType = (request, @base) => typeof(TwitterErrors),
							UserAgent = userAgent,
							Proxy = Proxy,
#if !SILVERLIGHT && !WINRT
							FollowRedirects = true,
#endif
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
						};

            _userStreamsClient = new RestClient
            {
                Authority = Globals.UserStreamsAuthority,
                Proxy = Proxy,
                VersionPath = "1.1",
                Serializer = serializer ?? jsonSerializer,
                Deserializer = deserializer ?? jsonSerializer,
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterErrors),
                UserAgent = userAgent,
#if !SILVERLIGHT && !WINRT
                FollowRedirects = true,
#endif
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
            };

            _publicStreamsClient = new RestClient
            {
                Authority = Globals.PublicStreamsAuthority,
                Proxy = Proxy,
                VersionPath = "1.1",
                Serializer = serializer ?? jsonSerializer,
                Deserializer = deserializer ?? jsonSerializer,
                DecompressionMethods = DecompressionMethods.GZip,
                UserAgent = userAgent,
#if !SILVERLIGHT && !WINRT
                FollowRedirects = true,
#endif
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
						};

            InitializeService();
        }

        private void InitializeService()
        {
            IncludeEntities = true;
            IncludeRetweets = true;
        }

        private readonly Func<RestRequest> _noAuthQuery
            = () =>
                  {
                      var request = new RestRequest();
                      return request;
                  };

        private readonly TwitterClientInfo _info;

        private RestRequest PrepareHammockQuery(string path)
        {
            RestRequest request;
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_tokenSecret))
            {
                request = _noAuthQuery.Invoke();
            }
            else
            {
                var args = new FunctionArguments
                {
                    ConsumerKey = _consumerKey,
                    ConsumerSecret = _consumerSecret,
                    Token = _token,
                    TokenSecret = _tokenSecret
                };
                request = _protectedResourceQuery.Invoke(args);
            }
            request.Path = path;

            SetTwitterClientInfo(request);

            // A little hacky, but these URLS have never changed
            if (path.Contains("account/update_profile_background_image") ||
                path.Contains("account/update_profile_image"))
            {
                PrepareUpload(request, path);
            }

            request.TraceEnabled = TraceEnabled;
            return request;
        }

        private static void PrepareUpload(RestBase request, string path)
        {
            //account/update_profile_image.json?image=[FILE_PATH]&include_entities=1
            var startIndex = path.IndexOf("?image_path=", StringComparison.Ordinal) + 12;
            var endIndex = path.IndexOf("&", StringComparison.Ordinal);
            var uri = path.Substring(startIndex, endIndex - startIndex);
            path = path.Replace(string.Format("image_path={0}&", uri), "");
            request.Path = path;
            request.Method = WebMethod.Post;
#if !WINRT
            request.AddFile("image", Path.GetFileName(Uri.UnescapeDataString(uri)), Path.GetFullPath(Uri.UnescapeDataString(uri)), "multipart/form-data");
#else
					var fullPath = Uri.UnescapeDataString(uri);
					if (!System.IO.Path.IsPathRooted(fullPath)) //Best guess at how to create a 'full' path on WinRT where file access is restricted and all paths should be passed as 'full' versions anyway.
						fullPath = System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, uri);
					request.AddFile("image", Path.GetFileName(Uri.UnescapeDataString(uri)), fullPath, "multipart/form-data");
#endif
        }

        private void SetTwitterClientInfo(RestBase request)
        {
            if (_info == null) return;
            if(!_info.ClientName.IsNullOrBlank())
            {
                request.AddHeader("X-Twitter-Name", _info.ClientName);
                request.UserAgent = _info.ClientName;
            }
            if (!_info.ClientVersion.IsNullOrBlank())
            {
                request.AddHeader("X-Twitter-Version", _info.ClientVersion);
            }
            if (!_info.ClientUrl.IsNullOrBlank())
            {
                request.AddHeader("X-Twitter-URL", _info.ClientUrl);
            }
        }

        public T Deserialize<T>(ITwitterModel model) where T : ITwitterModel
        {
            return Deserialize<T>(model.RawSource);
        }

        public T Deserialize<T>(string content)
        {
            var response = new RestResponse<T> { StatusCode = HttpStatusCode.OK };
            response.SetContent(content);
            return Deserializer.Deserialize<T>(response);
        }

        internal string FormatAsString { get; private set; }

        private string ResolveUrlSegments(string path, List<object> segments)
        {
            if (segments == null) throw new ArgumentNullException("segments");

            var cleansed = new List<object>();
            for (var i = 0; i < segments.Count; i++)
            {
                if (i == 0)
                {
                    cleansed.Add(segments[i]);
                }
                if (i > 0 && i % 2 == 0)
                {
                    var key = segments[i - 1];
                    var value = segments[i];    
                    if (value != null)
                    {
                        if (cleansed.Count == 1 && key is string)
                        {
                            var keyString = key.ToString();
                            if (keyString.StartsWith("&"))
                            {
                                key = "?" + keyString.Substring(1);
                            }
                        }
                        cleansed.Add(key);
                        cleansed.Add(value);
                    }
                }
            }
            segments = cleansed;

            for (var i = 0; i < segments.Count; i++)
            {
                if (segments[i] is DateTime)
                {
                    segments[i] = ((DateTime) segments[i]).ToString("yyyy-MM-dd");
                }

                if (segments[i] is bool)
                {
                    var flag = (bool) segments[i];
                    segments[i] = flag ? "1" : "0";
                }

                if(segments[i] is double)
                {
                    segments[i] = ((double) segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is decimal)
                {
                    segments[i] = ((decimal)segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is float)
                {
                    segments[i] = ((float)segments[i]).ToString(CultureInfo.InvariantCulture);
                }

                if (segments[i] is IEnumerable && !(segments[i] is string))
                {
                    ResolveEnumerableUrlSegments(segments, i);
                }
            }

            path = PathHelpers.ReplaceUriTemplateTokens(segments, path);

            PathHelpers.EscapeDataContainingUrlSegments(segments);

            const string includeEntities = "include_entities";
            const string includeRetweets = "include_rts";

            if (IncludeEntities && !IsKeyAlreadySet(segments, includeEntities))
            {
                segments.Add(segments.Count() > 1 ? "&" + includeEntities + "=" : "?" + includeEntities + "=");
                segments.Add("1");
            }
            if (IncludeRetweets && !IsKeyAlreadySet(segments, includeRetweets))
            {
                segments.Add(segments.Count() > 1 ? "&" + includeRetweets + "=" : "?" + includeRetweets + "=");
                segments.Add("1");
            }

            segments.Insert(0, path);
#if !WINRT
            return string.Concat(segments.ToArray()).ToString(CultureInfo.InvariantCulture);
#else
						return string.Concat(segments.ToArray());
#endif
        }

        private static bool IsKeyAlreadySet(IList<object> segments, string key)
        {
            for (var i = 1; i < segments.Count; i++)
            {
                if (i % 2 != 1 || !(segments[i] is string)) continue;
                var segment = ((string)segments[i]).Trim(new[] { '&', '=', '?' });

                if (!segment.Contains(key)) continue;
                return true;
            }
            return false;
        }

        private static void ResolveEnumerableUrlSegments(IList<object> segments, int i)
        {
            // [DC] Enumerable segments will be typed, but we only care about string values
            var collection = (from object item in (IEnumerable) segments[i] select item.ToString()).ToList();
            var total = collection.Count();
            var sb = new StringBuilder();
            var count = 0;
            foreach (var item in collection)
            {
                sb.Append(item);
                if (count < total - 1)
                {
                    sb.Append(",");
                }
                count++;
            }
            segments[i] = sb.ToString();
        }

#if !WINDOWS_PHONE
        private IAsyncResult WithHammock<T>(RestClient client, Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);

            return WithHammockImpl(client, request, action);
        }

				private IAsyncResult WithHammock<T>(RestClient client, Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            return WithHammock(client, action, ResolveUrlSegments(path, segments.ToList()));
        }

        private IAsyncResult WithHammock<T>(RestClient client, WebMethod method, Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);
            request.Method = method;

            return WithHammockImpl(client, request, action);
        }

				private IAsyncResult WithHammock<T>(RestClient client, WebMethod method, Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            return WithHammock(client, method, action, ResolveUrlSegments(path, segments.ToList()));
        }

        private IAsyncResult WithHammockImpl<T>(RestClient client, RestRequest request, Action<T, TwitterResponse> action) where T : class
        {
            return client.BeginRequest(
                request, new RestCallback<T>((req, response, state) =>
                {
                    if (response == null)
                    {
                        return;
                    }
                    SetResponse(response);
                    var entity = response.ContentEntity;
                    action.Invoke(entity, new TwitterResponse(response));
                }));
        }

				private IAsyncResult BeginWithHammock<T>(RestClient client, WebMethod method, string path, params object[] segments)
        {
            path = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(path);
            request.Method = method;
            var result = _client.BeginRequest<T>(request);
            return result;
        }

        private IAsyncResult BeginWithHammock<T>(RestClient client, WebMethod method, string path, IDictionary<string, Stream> files,params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(url);
            request.Method = method;
            request.QueryHandling = QueryHandling.AppendToParameters;
            foreach (var file in files)
            {
                request.AddFile("media", file.Key, file.Value);
            }
            var result = _client.BeginRequest<T>(request);
            return result;
        }

				private IAsyncResult BeginWithHammock<T>(RestClient client, WebMethod method, string path, MediaFile media, params object[] segments)
				{
					var url = ResolveUrlSegments(path, segments.ToList());
					var request = PrepareHammockQuery(url);
					request.Method = method;
					request.QueryHandling = QueryHandling.AppendToParameters;
					request.AddFile("media", media.FileName, media.Content);
					var result = _client.BeginRequest<T>(request);
					return result;
				}

        private T EndWithHammock<T>(IAsyncResult result)
        {
            var response = _client.EndRequest<T>(result);
            SetResponse(response);
            return response.ContentEntity;
        }

        private T EndWithHammock<T>(IAsyncResult result, TimeSpan timeout)
        {
            var response = _client.EndRequest<T>(result, timeout);
            return response.ContentEntity;
        }
#endif

#if !SILVERLIGHT && !WINRT
				private T WithHammock<T>(RestClient client, string path)
        {
            var request = PrepareHammockQuery(path);

            return WithHammockImpl<T>(client, request);
        }

				private T WithHammock<T>(RestClient client, string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            return WithHammock<T>(client, url);
        }

				private T WithHammock<T>(RestClient client, WebMethod method, string path)
        {
            var request = PrepareHammockQuery(path);
            request.Method = method;

            return WithHammockImpl<T>(client, request);
        }

				private T WithHammock<T>(RestClient client, WebMethod method, string path, IDictionary<string, Stream> files, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(url);
            request.Method = method;
            request.QueryHandling = QueryHandling.AppendToParameters;
            foreach (var file in files)
            {
                request.AddFile("media",file.Key, file.Value);
            }
            return WithHammockImpl<T>(client, request);
        }

				private T WithHammock<T>(RestClient client, WebMethod method, string path, MediaFile media, params object[] segments)
				{
					var url = ResolveUrlSegments(path, segments.ToList());
					var request = PrepareHammockQuery(url);
					request.Method = method;
					request.QueryHandling = QueryHandling.AppendToParameters;

					request.AddFile("media", media.FileName, media.Content);
					return WithHammockImpl<T>(client, request);
				}

				private T WithHammock<T>(RestClient client, WebMethod method, string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());

            return WithHammock<T>(client, method, url);
        }

				private T WithHammockImpl<T>(RestRequest request)
				{
					return WithHammockImpl<T>(_client, request);
				}

        private T WithHammockImpl<T>(RestClient client, RestRequest request)
        {
					var response = client.Request<T>(request);
            
            SetResponse(response);

            var entity = response.ContentEntity;
            return entity;
        }
#endif

#if WINDOWS_PHONE 
        private void WithHammock<T>(Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);
            
            WithHammockImpl(request, action);
        }
        
        private void WithHammock<T>(Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            WithHammock(action, ResolveUrlSegments(path, segments.ToList()));
        }

        private void WithHammock<T>(WebMethod method, Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);
            request.Method = method;

            WithHammockImpl(request, action);
        }

        private void WithHammock<T>(WebMethod method, Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            WithHammock(method, action, ResolveUrlSegments(path, segments.ToList()));
        }

        private void WithHammockImpl<T>(RestRequest request, Action<T, TwitterResponse> action) where T : class
        {
            _client.BeginRequest(
                request, new RestCallback<T>((req, response, state) =>
                {
                    if (response == null)
                    {
                        return;
                    }
                    var entity = response.ContentEntity;
                    action.Invoke(entity, new TwitterResponse(response));
                }));
        }

        private void WithHammock<T>(WebMethod method, Action<T, TwitterResponse> action, string path, IDictionary<string, Stream> files, params object[] segments) where T : class
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(url);
            request.Method = method;
            request.QueryHandling = QueryHandling.AppendToParameters;
            foreach (var file in files)
            {
                request.AddFile("media", file.Key, file.Value);
            }
            WithHammockImpl(request, action);
        }
#endif

#if PLATFORM_SUPPORTS_ASYNC_AWAIT
				private Task<TwitterAsyncResult<T1>> WithHammockTask<T1>(RestClient client, string path, params object[] segments) where T1 : class
				{
					var tcs = new TaskCompletionSource<TwitterAsyncResult<T1>>();
					WithHammock(client,
						(Action<T1, TwitterResponse>)((v, r) =>
						{
							tcs.SetResult(new TwitterAsyncResult<T1>(v, r));
						}),
						path,
						segments
					);

					return tcs.Task;
				}

				private Task<TwitterAsyncResult<T1>> WithHammockTask<T1>(RestClient client, WebMethod method, string path, params object[] segments) where T1 : class
				{
					var tcs = new TaskCompletionSource<TwitterAsyncResult<T1>>();
					WithHammock(client, method,
						(Action<T1, TwitterResponse>)((v, r) =>
						{
							tcs.SetResult(new TwitterAsyncResult<T1>(v, r));
						}),
						path,
						segments
					);

					return tcs.Task;
				}

#endif

				private static T TryAsyncResponse<T>(Func<T> action, out Exception exception)
        {
            exception = null;
            var entity = default(T);
            try
            {
                entity = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return entity;
        }
    }
}
