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
        private readonly JsonSerializer _json;

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
                _oauth.UserAgent = value;
            }
        }

        private IDeserializer _customDeserializer;
        private ISerializer _customSerializer;
        public IDeserializer Deserializer
        {
            get
            {
                return _customDeserializer ?? _json;
            }
            set { _customDeserializer = value; }
        }

        public ISerializer Serializer
        {
            get
            {
                return _customSerializer ?? _json;
            }
            set { _customSerializer = value; }
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

 #if !SILVERLIGHT
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
        
        public TwitterService(string consumerKey, string consumerSecret, string proxy) : this(proxy)
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

        public TwitterService(string proxy = null)
        {
            this.Proxy = proxy;
            _json = new JsonSerializer();

            FormatAsString = ".json";

            _oauth = new RestClient
            {
                Authority = Globals.Authority,
                Proxy = Proxy,
                UserAgent = "TweetSharp",
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterError),
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
            };

            _client = new RestClient
            {
                Authority = Globals.Authority,
                QueryHandling = QueryHandling.AppendToParameters,
                VersionPath = "1.1",
                Serializer = _json,
                Deserializer = _json,
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterError),
                UserAgent = "TweetSharp",
                Proxy = Proxy,
#if !SILVERLIGHT
                FollowRedirects = true,
#endif
#if SILVERLIGHT
                HasElevatedPermissions = true
#endif
            };

            _userStreamsClient = new RestClient
            {
                Authority = Globals.UserStreamsAuthorityt,
                Proxy = Proxy,
                VersionPath = "1.1",
                Serializer = _json,
                Deserializer = _json,
                DecompressionMethods = DecompressionMethods.GZip,
                GetErrorResponseEntityType = (request, @base) => typeof(TwitterError),
                UserAgent = "TweetSharp",
#if !SILVERLIGHT
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
                Serializer = _json,
                Deserializer = _json,
                DecompressionMethods = DecompressionMethods.GZip,
                UserAgent = "TweetSharp",
#if !SILVERLIGHT
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
            request.AddFile("image", Path.GetFileName(Uri.UnescapeDataString(uri)), Path.GetFullPath(Uri.UnescapeDataString(uri)), "multipart/form-data");
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
            return _customDeserializer != null
                       ? _customDeserializer.Deserialize<T>(response)
                       : (T)_json.DeserializeContent(content, typeof(T));
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

            return string.Concat(segments.ToArray()).ToString(CultureInfo.InvariantCulture);
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
        private IAsyncResult WithHammock<T>(Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);

            return WithHammockImpl(request, action);
        }

        private IAsyncResult WithHammock<T>(Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            return WithHammock(action, ResolveUrlSegments(path, segments.ToList()));
        }

        private IAsyncResult WithHammock<T>(WebMethod method, Action<T, TwitterResponse> action, string path) where T : class
        {
            var request = PrepareHammockQuery(path);
            request.Method = method;

            return WithHammockImpl(request, action);
        }

        private IAsyncResult WithHammock<T>(WebMethod method, Action<T, TwitterResponse> action, string path, params object[] segments) where T : class
        {
            return WithHammock(method, action, ResolveUrlSegments(path, segments.ToList()));
        }

        private IAsyncResult WithHammockImpl<T>(RestRequest request, Action<T, TwitterResponse> action) where T : class
        {
            return _client.BeginRequest(
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
        
        private IAsyncResult BeginWithHammock<T>(WebMethod method, string path, params object[] segments)
        {
            path = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(path);
            request.Method = method;
            var result = _client.BeginRequest<T>(request);
            return result;
        }

        private IAsyncResult BeginWithHammock<T>(WebMethod method, string path, IDictionary<string, Stream> files,params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(url);
            request.Method = method;
            request.QueryHandling = QueryHandling.AppendToParameters;
            foreach (var file in files)
            {
                request.AddFile("media[]", file.Key, file.Value);
            }
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

#if !SILVERLIGHT
        private T WithHammock<T>(string path)
        {
            var request = PrepareHammockQuery(path);

            return WithHammockImpl<T>(request);
        }

        private T WithHammock<T>(string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            return WithHammock<T>(url);
        }

        private T WithHammock<T>(WebMethod method, string path)
        {
            var request = PrepareHammockQuery(path);
            request.Method = method;

            return WithHammockImpl<T>(request);
        }

        private T WithHammock<T>(WebMethod method, string path, IDictionary<string, Stream> files, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());
            var request = PrepareHammockQuery(url);
            request.Method = method;
            request.QueryHandling = QueryHandling.AppendToParameters;
            foreach (var file in files)
            {
                request.AddFile("media[]",file.Key, file.Value);
            }
            return WithHammockImpl<T>(request);
        }

        private T WithHammock<T>(WebMethod method, string path, params object[] segments)
        {
            var url = ResolveUrlSegments(path, segments.ToList());

            return WithHammock<T>(method, url);
        }

        private T WithHammockImpl<T>(RestRequest request)
        {
            var response = _client.Request<T>(request);
            
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
