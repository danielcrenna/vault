using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Hammock.Attributes.Specialized;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Validation;
using Hammock.Web.Mocks;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
using System.IO.IsolatedStorage;
#endif

#if SILVERLIGHT && !WindowsPhone
using System.Windows.Browser;
using System.Net.Browser;
#endif

namespace Hammock.Web
{
    public abstract partial class WebQuery: IDisposable
    {
        private const string AcceptEncodingHeader = "Accept-Encoding";
        private static readonly object _sync = new object();
        private readonly WebHeaderCollection _restrictedHeaders = new WebHeaderCollection(0);

        public virtual Encoding Encoding { get; protected internal set; }
        public virtual IWebQueryInfo Info { get; protected set; }
        public virtual string UserAgent { get; protected internal set; }
        public virtual WebHeaderCollection Headers { get; protected set; }
        public virtual WebParameterCollection Parameters { get; protected set; }
        [Obsolete("Use CookieContainer instead.")]
        public virtual WebParameterCollection Cookies { get; protected set; }
#if !NETCF
        public virtual CookieContainer CookieContainer { get; set; }
#endif
        private WebEntity _entity;
        protected internal virtual WebEntity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
                HasEntity = _entity != null;
            }
        }
        
        public virtual WebMethod Method { get; set; }
        public virtual string Proxy { get; set; }
        public virtual string AuthorizationHeader { get; internal set; }
        public DecompressionMethods? DecompressionMethods { get; set; }
        public virtual TimeSpan? RequestTimeout { get; set; }
        public virtual WebQueryResult Result { get; internal set; }
        public virtual object UserState { get; internal set; }

#if SILVERLIGHT
        public virtual bool HasElevatedPermissions { get; set; }

        // [DC]: Headers to use when access isn't direct
        public virtual string SilverlightUserAgentHeader { get; set; }
        public virtual string SilverlightAcceptEncodingHeader { get; set; }        
#endif
        
#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
        public virtual bool KeepAlive { get; set; }
        public virtual bool FollowRedirects { get; internal set; }
#endif

        private WebResponse _webResponse;
        public virtual WebResponse WebResponse
        {
            get
            {
                lock (_sync)
                {
                    return _webResponse;
                }
            }
            set
            {
                lock (_sync)
                {
                    _webResponse = value;
                }
            }
        }

        protected virtual Stream ContentStream { get; set; }
        public virtual bool HasEntity { get; set; }
        public virtual byte[] PostContent { get; set; }

#if SL3 || SL4
        static WebQuery()
        {
            // [DC]: Opt-in to the networking stack so we can get headers for proxies
            WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
        }
#endif

        protected WebQuery(bool enableTrace) : this(null, enableTrace)
        {

        }

        protected WebQuery(IWebQueryInfo info, bool enableTrace)
        {
            TraceEnabled = enableTrace;
            SetQueryMeta(info);
            InitializeResult();
        }

        protected bool TraceEnabled { get; private set; }

        private void SetQueryMeta(IWebQueryInfo info)
        {
#pragma warning disable 618
            Cookies = new WebParameterCollection(0);
#pragma warning restore 618

            if(info == null)
            {
                Headers = new WebHeaderCollection(0);
                Parameters = new WebParameterCollection(0);
                return;
            }

            Info = info;
            IEnumerable<PropertyInfo> properties;
            IDictionary<string, string> transforms;

            ParseTransforms(out properties, out transforms);
            Headers = ParseInfoHeaders(properties, transforms);
            Parameters = ParseInfoParameters(properties, transforms);
            ParseUserAgent(properties);
            ParseWebEntity(properties);
        }

        private void ParseTransforms(out IEnumerable<PropertyInfo> properties, 
                                     out IDictionary<string, string> transforms)
        {
            properties = Info.GetType().GetProperties();
            transforms = new Dictionary<string, string>(0);
            Info.ParseValidationAttributes(properties, transforms);
        }

        private void InitializeResult()
        {
            Result = new WebQueryResult();
            QueryRequest += (s, e) => SetRequestResults(e);
            QueryResponse += (s, e) => SetResponseResults(e);
        }

        private void SetResponseResults(WebQueryResponseEventArgs e)
        {
            Result.ContentStream = e.Response;
            Result.RequestHttpMethod = Method.ToUpper();
            Result.IsMock = WebResponse is MockHttpWebResponse;
            Result.TimedOut = TimedOut;

            string version;
            int statusCode;
            string statusDescription;
            System.Net.WebHeaderCollection headers;
            string contentType;
            long contentLength;
            Uri responseUri;
            CastWebResponse(
                out version, out statusCode, out statusDescription, out headers, 
                out contentType, out contentLength, out responseUri
                );
			
#if !MonoTouch
            TraceResponse(
                responseUri, version, headers, statusCode, statusDescription
                );
#endif

            Result.WebResponse = WebResponse;
            Result.ResponseHttpStatusCode = statusCode;
            Result.ResponseHttpStatusDescription = statusDescription;
            Result.ResponseType = contentType;
            Result.ResponseLength = contentLength;
            Result.ResponseUri = responseUri;
            Result.Exception = e.Exception;
            if (WebResponse != null)
            {
                Result.ResponseDate = DateTime.UtcNow;
            }
        }

#if !MonoTouch
		[Conditional("TRACE")]
        private void TraceResponse(Uri uri, string version, System.Net.WebHeaderCollection headers, int statusCode, string statusDescription)
        {
            if(!TraceEnabled)
            {
                return;
            }

            Trace.WriteLine(
                String.Concat("\r\n--RESPONSE:", " ", uri)
                );
            Trace.WriteLine(
                String.Concat(version, " ", statusCode, " ", statusDescription)
                );
            foreach (var trace in headers.AllKeys.Select(
                key => String.Concat(key, ": ", headers[key])))
            {
                Trace.WriteLine(trace);
            }
        }
#endif

        private void SetRequestResults(WebQueryRequestEventArgs e)
        {
            Result.RequestDate = DateTime.UtcNow;
            Result.RequestUri = new Uri(e.Request);
#if !SILVERLIGHT
            Result.RequestKeptAlive = KeepAlive;
#endif
        }

#if !SILVERLIGHT
        protected virtual void SetWebProxy(WebRequest request)
        {
#if !Smartphone && !NETCF
            var proxyUriBuilder = new UriBuilder(Proxy);
            request.Proxy = new WebProxy(proxyUriBuilder.Host,
                                         proxyUriBuilder.Port);

            if (!proxyUriBuilder.UserName.IsNullOrBlank())
            {
                request.Headers["Proxy-Authorization"] = WebExtensions.ToBasicAuthorizationHeader(proxyUriBuilder.UserName,
                                                                                             proxyUriBuilder.Password);
            }
#else
          var uri = new Uri(Proxy);
            request.Proxy = new WebProxy(uri.Host, uri.Port);
            var userParts = uri.UserInfo.Split(new[] { ':' }).Where(ui => !ui.IsNullOrBlank()).ToArray();
            if (userParts.Length == 2)
            {
                request.Proxy.Credentials = new NetworkCredential(userParts[0], userParts[1]);
            }
#endif
        }
#endif

        protected virtual WebRequest BuildPostOrPutWebRequest(PostOrPut method, string url, out byte[] content)
        {
            return !HasEntity
                       ? BuildPostOrPutFormWebRequest(method, url, out content)
                       : BuildPostOrPutEntityWebRequest(method, url, out content);
        }

        protected virtual Func<string, string> BeforeBuildPostOrPutFormWebRequest()
        {
            return url => AppendParameters(url).Replace(url + "?", "");
        }
        
        protected virtual WebRequest BuildPostOrPutFormWebRequest(PostOrPut method, string url, out byte[] content)
        {
            var post = BeforeBuildPostOrPutFormWebRequest().Invoke(url);

            var request = WebRequest.Create(url);

            AuthenticateRequest(request);

            SetMethod(method.ToString(), request);

            // It should be possible to override the content type in the case of AddPostContent
            var hasContentType = Headers.AllKeys.Where(
                key => key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase)
                ).Count() > 0;
            
            if(!hasContentType)
            {
                request.ContentType = "application/x-www-form-urlencoded";
            }

            HandleRequestMeta(request);
			
#if !MonoTouch
            TraceRequest(request);
#endif
            content = BuildPostOrPutContent(request, post);

#if !SILVERLIGHT
            request.ContentLength = content.Length;
#endif
            return request;
        }

        protected virtual byte[] BuildPostOrPutContent(WebRequest request, string post)
        {
            var encoding = Encoding ?? Encoding.UTF8;

            var content = PostContent ?? encoding.GetBytes(post);

#if TRACE
            Trace.WriteLineIf(TraceEnabled, string.Concat("\r\n", content));            
#endif
			return content;
        }

        protected virtual Func<string, string> BeforeBuildPostOrPutEntityWebRequest()
        {
            return AppendParameters;
        }

        protected virtual WebRequest BuildPostOrPutEntityWebRequest(PostOrPut method, string url, out byte[] content)
        {
            url = BeforeBuildPostOrPutEntityWebRequest().Invoke(url);

            var request = WebRequest.Create(url);

            SetMethod(method.ToString(), request);
            
            AuthenticateRequest(request);

            HandleRequestMeta(request);
			
#if !MonoTouch
            TraceRequest(request);
#endif

            if (Entity != null)
            {
                var entity = Entity.Content;

                var encoding = Entity.ContentEncoding ?? Encoding ?? Encoding.UTF8;

                content = encoding.GetBytes(entity);

                request.ContentType = Entity.ContentType;
#if TRACE
                Trace.WriteLineIf(TraceEnabled, string.Concat("\r\n", entity));
#endif
                
#if !SILVERLIGHT 
                // [DC]: This is set by Silverlight
                request.ContentLength = content.Length;
#endif
            }
            else
            {
                using(var ms = new MemoryStream())
                {
                    content = ms.ToArray();
                }
            }

            return request;
        }

        protected virtual Func<string, string> BeforeBuildGetDeleteHeadOptionsWebRequest()
        {
            return AppendParameters;
        }

        protected virtual WebRequest BuildGetDeleteHeadOptionsWebRequest(GetDeleteHeadOptions method, string url)
        {
            url = BeforeBuildGetDeleteHeadOptionsWebRequest().Invoke(url);

            var request = WebRequest.Create(url);

            SetMethod(method.ToString(), request);
            
            AuthenticateRequest(request);

            HandleRequestMeta(request);
			
#if !MonoTouch
            TraceRequest(request);
#endif
            return request;
        }

        private void SetMethod(string method, WebRequest request)
        {
            request.Method = method.ToUpper();
        }

        private void HandleRequestMeta(WebRequest request)
        {
            // [DC] LSP violation necessary for "pure" mocks
            if (request is HttpWebRequest)
            {
                SetRequestMeta((HttpWebRequest)request);
            }
            else
            {
                AppendHeaders(request);
                SetUserAgent(request);
            }
        }

        protected virtual void SetUserAgent(WebRequest request)
        {
            if (!UserAgent.IsNullOrBlank())
            {
#if SILVERLIGHT && !WindowsPhone
                // [DC] User-Agent is still restricted in elevated mode
                request.Headers[SilverlightUserAgentHeader ?? "X-User-Agent"] = UserAgent;
#else
                if(request is HttpWebRequest)
                {
                    ((HttpWebRequest) request).UserAgent = UserAgent;
                }
                else
                {
                    request.Headers["User-Agent"] = UserAgent;
                }
#endif
            }
        }

        protected virtual void SetRequestMeta(HttpWebRequest request)
        {
            AppendHeaders(request);
            AppendCookies(request);

#if !SILVERLIGHT
            if (ServicePoint != null)
            {
#if !Smartphone  && !NETCF
                request.ServicePoint.ConnectionLeaseTimeout = ServicePoint.ConnectionLeaseTimeout;
                request.ServicePoint.ReceiveBufferSize = ServicePoint.ReceiveBufferSize;
                request.ServicePoint.UseNagleAlgorithm = ServicePoint.UseNagleAlgorithm;
                request.ServicePoint.BindIPEndPointDelegate = ServicePoint.BindIPEndPointDelegate;
#endif
              request.ServicePoint.ConnectionLimit = ServicePoint.ConnectionLimit;
                request.ServicePoint.Expect100Continue = ServicePoint.Expect100Continue;
                request.ServicePoint.MaxIdleTime = ServicePoint.MaxIdleTime;
            }
#endif

#if !SILVERLIGHT
            if (!Proxy.IsNullOrBlank())
            {
                SetWebProxy(request);
            }
            request.AllowAutoRedirect = FollowRedirects;
#endif

            SetUserAgent(request);

            if (DecompressionMethods.HasValue)
            {
                var decompressionMethods = DecompressionMethods.Value;

#if !SILVERLIGHT && !WindowsPhone
                request.AutomaticDecompression = decompressionMethods;
#else

#if !WindowsPhone
                if (HasElevatedPermissions)
                {
#endif
                switch (decompressionMethods)
                {
                    case Silverlight.Compat.DecompressionMethods.GZip:
                        request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "gzip";
                        break;
                    case Silverlight.Compat.DecompressionMethods.Deflate:
                        request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "deflate";
                        break;
                    case Silverlight.Compat.DecompressionMethods.GZip | Silverlight.Compat.DecompressionMethods.Deflate:
                        request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "gzip,deflate";
                        break;
                }

#if !WindowsPhone
                }
                else
                {
                    switch (decompressionMethods)
                    {
                        case Silverlight.Compat.DecompressionMethods.GZip:
                            request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "gzip";
                            break;
                        case Silverlight.Compat.DecompressionMethods.Deflate:
                            request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "deflate";
                            break;
                        case Silverlight.Compat.DecompressionMethods.GZip | Silverlight.Compat.DecompressionMethods.Deflate:
                            request.Headers[SilverlightAcceptEncodingHeader ?? "X-Accept-Encoding"] = "gzip,deflate";
                            break;
                    }
                }
#endif

#endif
            }
#if !SILVERLIGHT
            if (RequestTimeout.HasValue)
            {
                // [DC] Need to synchronize these as Timeout is ignored in async requests
                request.Timeout = (int)RequestTimeout.Value.TotalMilliseconds;
                request.ReadWriteTimeout = (int)RequestTimeout.Value.TotalMilliseconds;
            }

            if (KeepAlive)
            {
                request.KeepAlive = true;
            }
#endif
        }

        private void AppendCookies(HttpWebRequest request)
        {
#if !NETCF
            if (this.CookieContainer != null)
            {
                request.CookieContainer = this.CookieContainer;
            }
            else
            {
                // [MK] This line creates serious problems if the following two conditions are true:
                // - Silverlight out of browser client with elevated privileges (hence going directly for the resource, bypassing clientaccesspolicy.xml and such)
                // - There are no cookies
                // - The remote server is down
                // Given these conditions the Silverlight client HTTP stack throws ArgumentNullException inside
                // HttpWebRequestHelper.ParseHeaders - go figure.
                //request.CookieContainer = new CookieContainer();

                var cookieContainer = new CookieContainer();
#pragma warning disable 618
                foreach (var cookie in Cookies.OfType<HttpCookieParameter>())
#pragma warning restore 618
                {
                    var value = new Cookie(cookie.Name, cookie.Value);
                    if (cookie.Domain != null)
                    {
                        cookieContainer.Add(cookie.Domain, value);
                    }
#if !SILVERLIGHT
                    else
                    {
                        request.CookieContainer.Add(value);
                    }
#endif
                }

                if (cookieContainer.Count > 0)
                {
                    request.CookieContainer = cookieContainer;
                }
            }
#endif
        }

        protected virtual void AppendHeaders(WebRequest request)
        {
            if (!(request is HttpWebRequest) &&
                !(request is MockHttpWebRequest))
            {
                return;
            }

            // [DC]: Combine all duplicate headers into CSV
            var headers = new Dictionary<string, string>(0);
            foreach(var header in Headers)
            {
                string value;
                if(headers.ContainsKey(header.Name))
                {
                    value = String.Concat(headers[header.Name], ",", header.Value);
                    headers.Remove(header.Name);
                }
                else
                {
                    value = header.Value;
                }

                headers.Add(header.Name, value);
            }

            foreach (var header in headers)
            {
                if (_restrictedHeaderActions.ContainsKey(header.Key))
                {
                    if(request is HttpWebRequest)
                    {
#if SILVERLIGHT
                    if(header.Key.EqualsIgnoreCase("User-Agent"))
                    {
                        // [DC]: User-Agent is still restricted in elevated mode
                        request.Headers[SilverlightUserAgentHeader ?? "X-User-Agent"] = UserAgent;
                        continue;
                    }
#endif
                        _restrictedHeaderActions[header.Key].Invoke((HttpWebRequest) request, header.Value);
                        _restrictedHeaders.Add(header.Key, header.Value);
                    }
                    if(request is MockHttpWebRequest)
                    {
                        AddHeader(header, request);
                    }
                }
                else
                {
                    AddHeader(header, request);
                }
            }
        }
		
#if !MonoTouch
        [Conditional("TRACE")]
        private void TraceHeaders(WebRequest request)
        {
            if (!TraceEnabled)
            {
                return;
            }

            var restricted = _restrictedHeaders.AllKeys.Select(key => String.Concat(key, ": ", request.Headers[key]));
            var remaining = request.Headers.AllKeys.Except(_restrictedHeaders.AllKeys).Select(key => String.Concat(key, ": ", request.Headers[key]));
            var all = restricted.ToList();
            all.AddRange(remaining);
            all.Sort();

            foreach (var trace in all)
            {
                Trace.WriteLine(trace);
            }
        }
#endif

        private static void AddHeader(KeyValuePair<string, string> header, WebRequest request)
        {
#if !SILVERLIGHT
            request.Headers.Add(header.Key, header.Value);
#else
            request.Headers[header.Key] = header.Value;
#endif
        }

#if !SILVERLIGHT
        private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions
            = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase)
                  {
                      {"Accept", (r, v) => r.Accept = v},
                      {"Connection", (r, v) => r.Connection = v},
                      {"Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v)},
                      {"Content-Type", (r, v) => r.ContentType = v},
                      {"Expect", (r, v) => r.Expect = v},
                      {"Date", (r, v) => { /* Set by system */ }},
                      {"Host", (r, v) => { /* Set by system */ }},
                      {"If-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v)},
                      {"Range", (r, v) => { throw new NotSupportedException( /* r.AddRange() */); }},
                      {"Referer", (r, v) => r.Referer = v},
                      {"Transfer-Encoding", (r, v) => { r.TransferEncoding = v; r.SendChunked = true; }},
                      {"User-Agent", (r, v) => r.UserAgent = v }
                  };
#else
        private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions
            = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase) {
                      { "Accept",            (r, v) => r.Accept = v },
                      { "Connection",        (r, v) => { /* Set by Silverlight */ }},           
                      { "Content-Length",    (r, v) => { /* Set by Silverlight */ }},
                      { "Content-Type",      (r, v) => r.ContentType = v },
                      { "Expect",            (r, v) => { /* Set by Silverlight */ }},
                      { "Date",              (r, v) => { /* Set by Silverlight */ }},
                      { "Host",              (r, v) => { /* Set by Silverlight */ }},
                      { "If-Modified-Since", (r, v) => { /* Not supported */ }},
                      { "Range",             (r, v) => { /* Not supported */ }},
                      { "Referer",           (r, v) => { /* Not supported */ }},
                      { "Transfer-Encoding", (r, v) => { /* Not supported */ }},
                      { "User-Agent",        (r, v) => { /* Not supported here */  }}             
                  };
#endif

        protected virtual string AppendParameters(string url)
        {
            var count = 0;

            var parameters = Parameters.Where(
                parameter => !(parameter is HttpPostParameter) || Method == WebMethod.Post).Where(
                parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)
                );

            foreach (var parameter in parameters)
            {
                // GET parameters in URL
                url = url.Then(count > 0 || url.Contains("?") ? "&" : "?");
                url = url.Then("{0}={1}".FormatWith(parameter.Name, parameter.Value.UrlEncode()));
                count++;
            }

            return url;
        }

        // [DC] Headers don't need to be unique, this should change
        protected virtual WebHeaderCollection ParseInfoHeaders(IEnumerable<PropertyInfo> properties,
                                                               IDictionary<string, string> transforms)
        {
            var headers = new Dictionary<string, string>();
            
            Info.ParseNamedAttributes<HeaderAttribute>(properties, transforms, headers);

            var collection = new WebHeaderCollection();
            headers.ForEach(p => collection.Add(new WebHeader(p.Key, p.Value)));

            return collection;
        }

        protected virtual WebParameterCollection ParseInfoParameters(IEnumerable<PropertyInfo> properties,
                                                                     IDictionary<string, string> transforms)
        {
            var parameters = new Dictionary<string, string>();
            
            Info.ParseNamedAttributes<ParameterAttribute>(properties, transforms, parameters);

            var collection = new WebParameterCollection();
            parameters.ForEach(p => collection.Add(new WebParameter(p.Key, p.Value)));

            return collection;
        }

        protected virtual WebParameterCollection ParseInfoParameters()
        {
            IEnumerable<PropertyInfo> properties;
            IDictionary<string, string> transforms;
            ParseTransforms(out properties, out transforms);
            return ParseInfoParameters(properties, transforms);
        }

        private void ParseUserAgent(IEnumerable<PropertyInfo> properties)
        {
            var count = 0;
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<UserAgentAttribute>(true);
                count += attributes.Count();
                if (count > 1)
                {
                    throw new ArgumentException("Cannot declare more than one user agent per query");
                }

                if (count < 1)
                {
                    continue;
                }

                if (!UserAgent.IsNullOrBlank())
                {
                    continue;
                }

                var value = property.GetValue(Info, null);
                UserAgent = value != null ? value.ToString() : null;
            }
        }

        private void ParseWebEntity(IEnumerable<PropertyInfo> properties)
        {
            if (Entity != null)
            {
                // Already set by client or request
                return;
            }

            var count = 0;
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<EntityAttribute>(true);
                count += attributes.Count();
                if (count > 1)
                {
                    throw new ValidationException("Cannot declare more than one entity per query");
                }

                if (count < 1)
                {
                    continue;
                }

                if (Entity != null)
                {
                    // Already set in this pass
                    continue;
                }

                var value = property.GetValue(Info, null);

                var content = value != null ? value.ToString() : null;
                var contentEncoding = attributes.Single().ContentEncoding;
                var contentType = attributes.Single().ContentType;

                Entity = new WebEntity
                {
                    Content = content,
                    ContentEncoding = contentEncoding,
                    ContentType = contentType
                };
            }
        }

        protected void HandleWebException(WebException exception)
        {
            Stream stream = null;
            Stream emptyStream = new MemoryStream(new byte[] { });
                
            if (exception.Response is HttpWebResponse)
            {
                var response = exception.Response;
#if SILVERLIGHT
                if (DecompressionMethods == Silverlight.Compat.DecompressionMethods.GZip ||
                    DecompressionMethods == Silverlight.Compat.DecompressionMethods.Deflate ||
                    DecompressionMethods == (Silverlight.Compat.DecompressionMethods.GZip | Silverlight.Compat.DecompressionMethods.Deflate)
                    )
                {
                    response = new GzipHttpWebResponse((HttpWebResponse)response);
                }
#endif
                WebResponse = response;
                stream = WebResponse.GetResponseStream();
            }
            
            var args = new WebQueryResponseEventArgs(stream, exception);
            OnQueryResponse(args);
        }

        protected abstract void SetAuthorizationHeader(WebRequest request, string header);

        protected abstract void AuthenticateRequest(WebRequest request);

        public abstract string GetAuthorizationContent();

        private static string CreateCacheKey(string prefix, string url)
        {
            return !prefix.IsNullOrBlank() ? "{0}_{1}".FormatWith(prefix, url) : url;
        }

        protected virtual void ExecuteWithCache(ICache cache,
                                                string url,
                                                string key,
                                                Action<ICache, string> cacheScheme)
        {
            var fetch = cache.Get<Stream>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);
            }
            else
            {
                cacheScheme.Invoke(cache, url);
            }
        }

        protected virtual void ExecuteWithCacheAndAbsoluteExpiration(ICache cache,
                                                                       string url,
                                                                       string key,
                                                                       DateTime expiry,
                                                                       Action<ICache, string, DateTime> cacheScheme)
        {
            var fetch = cache.Get<Stream>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);
            }
            else
            {
                cacheScheme.Invoke(cache, url, expiry);
            }
        }

        protected virtual void ExecuteWithCacheAndSlidingExpiration(ICache cache,
                                                                      string url,
                                                                      string key,
                                                                      TimeSpan expiry,
                                                                      Action<ICache, string, TimeSpan> cacheScheme)
        {
            var fetch = cache.Get<Stream>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);
            }
            else
            {
                cacheScheme.Invoke(cache, url, expiry);    
            }
        }

#if !SILVERLIGHT
        protected virtual void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, string url, string key, ICache cache, out WebException exception)
        {
            WebException ex = null;
            ExecuteWithCache(cache, url, key, (c, u) => ExecuteGetDeleteHeadOptions(method, cache, url, key, out ex));
            exception = ex;
        }

        protected virtual void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, 
                                                             string url, 
                                                             string key, 
                                                             ICache cache, 
                                                             DateTime absoluteExpiration, 
                                                             out WebException exception)
        {
            WebException ex = null; 
            ExecuteWithCacheAndAbsoluteExpiration(cache, url, key, absoluteExpiration,
                                                            (c, u, e) =>
                                                            ExecuteGetDeleteHeadOptions(method, cache, url, key, absoluteExpiration, out ex));
            exception = ex;
        }

        protected virtual void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, 
                                                    string url, 
                                                    string key, 
                                                    ICache cache, 
                                                    TimeSpan slidingExpiration, 
                                                    out WebException exception)
        {
            WebException ex = null; 
            ExecuteWithCacheAndSlidingExpiration(cache, url, key, slidingExpiration,
                                                           (c, u, e) =>
                                                           ExecuteGetDeleteHeadOptions(method, cache, url, key, slidingExpiration, out ex));
            exception = ex;
        }

        private void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method,
                                                 ICache cache, 
                                                 string url, 
                                                 string key, 
                                                 out WebException exception)
        {
            ExecuteGetDeleteHeadOptions(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream);
            }
        }

        private void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, 
                                                 ICache cache, 
                                                 string url, 
                                                 string key,
                                                 DateTime absoluteExpiration, 
                                                 out WebException exception)
        {
            ExecuteGetDeleteHeadOptions(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream, absoluteExpiration);
            }
        }

        private void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, ICache cache, string url, string key,
                                                   TimeSpan slidingExpiration, out WebException exception)
        {
            ExecuteGetDeleteHeadOptions(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream, slidingExpiration);
            }
        }
#endif  

        public virtual event EventHandler<WebQueryRequestEventArgs> QueryRequest;
        public virtual void OnQueryRequest(WebQueryRequestEventArgs args)
        {
            var handler = QueryRequest;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public virtual event EventHandler<WebQueryResponseEventArgs> QueryResponse;
        public virtual void OnQueryResponse(WebQueryResponseEventArgs args)
        {
            var handler = QueryResponse;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        internal virtual event EventHandler<PostProgressEventArgs> PostProgress;
        internal virtual void OnPostProgress(PostProgressEventArgs args)
        {
            var handler = PostProgress;
            if (handler != null)
            {
                handler(this, args);
            }
        }
#if !SILVERLIGHT
        protected virtual void ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, string url, out WebException exception)
        {
            WebResponse = null;
            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            
            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            ExecuteGetDeleteHeadOptions(request, out exception);
        }

        private void ExecuteGetDeleteHeadOptions(WebRequest request, out WebException exception)
        {
            try
            {
                // [DC] Avoid disposing until no longer needed to build results
                var response = request.GetResponse();
                WebResponse = response;

                if (response != null)
                {
                    ContentStream = response.GetResponseStream();
                    if (ContentStream != null)
                    {
                        var args = new WebQueryResponseEventArgs(ContentStream);
                        OnQueryResponse(args);
                    }
                }

                exception = null;
            }
            catch (WebException ex)
            {
                exception = ex;
                HandleWebException(ex);
            }
        }
        
#endif
        protected virtual WebRequest BuildMultiPartFormRequest(PostOrPut method, string url, IEnumerable<HttpPostParameter> parameters, out string boundary)
        {
            url = BeforeBuildPostOrPutEntityWebRequest()(url);

            boundary = Guid.NewGuid().ToString();
            var request = WebRequest.Create(url);
            AuthenticateRequest(request);

            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
            
            HandleRequestMeta(request);
            
#if !MonoTouch
			TraceRequest(request);
#endif
			
            return request;
        }
		
#if !MonoTouch
        [Conditional("TRACE")]
        protected void TraceRequest(WebRequest request)
        {
            if (!TraceEnabled)
            {
                return;
            }

            var version = request is HttpWebRequest ?
#if SILVERLIGHT
                "HTTP/v1.1" :
#else
                string.Concat("HTTP/", ((HttpWebRequest)request).ProtocolVersion) :
#endif
 "HTTP/v1.1";
			
            Trace.WriteLine(
                String.Concat("--REQUEST: ", request.RequestUri.Scheme, "://", request.RequestUri.Host)
                );
            var pathAndQuery = String.Concat(
                request.RequestUri.AbsolutePath, string.IsNullOrEmpty(request.RequestUri.Query)
                                                     ? ""
                                                     : string.Concat(request.RequestUri.Query)
                );
            Trace.WriteLine(
                String.Concat(request.Method, " ", pathAndQuery, " ", version
                ));

            TraceHeaders(request);
        }
#endif

#if !SILVERLIGHT
        protected virtual void ExecutePostOrPut(PostOrPut method, string url, out WebException exception)
        {
            WebResponse = null;
            exception = null;
            byte[] post;
            var request = BuildPostOrPutWebRequest(method, url, out post);

            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(post, 0, post.Length);
                    stream.Close();

#if TRACE
                    var encoding = Encoding ?? new UTF8Encoding();
#if NETCF
                    Trace.WriteLineIf(TraceEnabled, encoding.GetString(post, 0, post.Length));
#else
                    Trace.WriteLineIf(TraceEnabled, encoding.GetString(post));
#endif
#endif

                    // [DC] Avoid disposing until no longer needed to build results
                    var response = request.GetResponse();
                    WebResponse = response;

                    if (response != null)
                    {
                        ContentStream = response.GetResponseStream();
                        if (ContentStream != null)
                        {
                            var args = new WebQueryResponseEventArgs(ContentStream);
                            OnQueryResponse(args);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex; 
                HandleWebException(ex);
            }
        }

        protected virtual void ExecutePostOrPut(PostOrPut method, 
                                                string url, 
                                                IEnumerable<HttpPostParameter> parameters,
                                                out WebException exception)
        {
            WebResponse = null;

            string boundary;
            var request = BuildMultiPartFormRequest(method, url, parameters, out boundary);

#if !Smartphone
            var encoding = Encoding ?? Encoding.GetEncoding("ISO-8859-1");
#else
            var encoding = Encoding ?? Encoding.GetEncoding(1252);
#endif
            var expected = WriteMultiPartImpl(
                false /* write */, parameters, boundary, encoding, null
                );

            request.ContentLength = expected;

            try
            {
                using (var requestStream = request.GetRequestStream())
                {
#if DEBUG
					var actual = WriteMultiPartImpl(
                        true /* write */, parameters, boundary, encoding, requestStream
                        );
					
                    Debug.Assert(expected == actual, string.Format("Expected {0} bytes but wrote {1}!", expected, actual));
#else
				WriteMultiPartImpl(
                        true /* write */, parameters, boundary, encoding, requestStream
                        );
#endif

                    // [DC] Avoid disposing until no longer needed to build results
                    var response = request.GetResponse();
                    WebResponse = response;

                    if (response != null)
                    {
                        ContentStream = response.GetResponseStream();
                        if (ContentStream != null)
                        {
                            var args = new WebQueryResponseEventArgs(ContentStream);
                            OnQueryResponse(args);
                        }
                    }

                    exception = null;
                }
            }
            catch (WebException ex)
            {
                exception = ex;
                HandleWebException(ex);
            }
        }
#endif

        private static int Write(bool write, Encoding encoding, Stream requestStream, string input)
        {
            var dataBytes = encoding.GetBytes(input);
            if(write)
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }
            return dataBytes.Length;
        }

        private static int WriteLine(bool write, Encoding encoding, Stream requestStream, string input)
        {
            var sb = new StringBuilder();
            sb.AppendLine(input);

            var dataBytes = encoding.GetBytes(sb.ToString());
            if (write)
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }
            return dataBytes.Length;
        }

        private long WriteMultiPartImpl(bool write, IEnumerable<HttpPostParameter> parameters, string boundary, Encoding encoding, Stream requestStream)
        {
            Stream fs = null;
            var header = string.Format("--{0}", boundary);
            var footer = string.Format("--{0}--", boundary);
            long written = 0;

            foreach (var parameter in parameters)
            {
                written += WriteLine(write, encoding, requestStream, header);
#if TRACE
                if(write)
                {
                    Trace.WriteLineIf(TraceEnabled, header);
                }
#endif
                switch (parameter.Type)
                {
                    case HttpPostParameterType.File:
                        {
                            var disposition = parameter.ContentDisposition ?? "form-data";
                            var fileMask = "Content-Disposition: " + disposition + "; name=\"{0}\"; filename=\"{1}\"";
                            var fileHeader = fileMask.FormatWith(parameter.Name, parameter.FileName);
                            var fileLine = "Content-Type: {0}".FormatWith(parameter.ContentType.ToLower());

                            written += WriteLine(write, encoding, requestStream, fileHeader);
                            written += WriteLine(write, encoding, requestStream, fileLine);
                            written += WriteLine(write, encoding, requestStream, "");
#if TRACE
                            if (write)
                            {
                                Trace.WriteLineIf(TraceEnabled, fileHeader);
                                Trace.WriteLineIf(TraceEnabled, fileLine);
                                Trace.WriteLineIf(TraceEnabled, "");
                                Trace.WriteLineIf(TraceEnabled, "[FILE DATA]");
                            }
#endif

#if !SILVERLIGHT
                            fs = parameter.FileStream ?? new FileStream(parameter.FilePath, FileMode.Open, FileAccess.Read);
#else
                            if (parameter.FileStream == null)
                            {
                                var store = IsolatedStorageFile.GetUserStoreForApplication();
                                var stream = store.OpenFile(parameter.FilePath, FileMode.Open, FileAccess.Read);
                                parameter.FileStream = stream;
                            }

                            fs = parameter.FileStream; // <-- WP7 requires a stream
#endif
                            {
                                if(!write)
                                {
                                    written += fs.Length;
                                }
                                else
                                {
                                    var fileWritten = default(long);
                                    using (var br = new BinaryReader(fs))
                                    {
                                        while (fileWritten < fs.Length)
                                        {
                                            var buffer = br.ReadBytes(8192);
                                            requestStream.Write(buffer, 0, buffer.Length);
                                            written += buffer.Length;
                                            fileWritten += buffer.Length;

                                            var args = new PostProgressEventArgs
                                            {
                                                FileName = parameter.FileName,
                                                BytesWritten = fileWritten,
                                                TotalBytes = fs.Length
                                            };
                                            OnPostProgress(args);
                                        }
                                    }
                                }
                            }
                            written += WriteLine(write, encoding, requestStream, "");
                            break;
                        }
                    case HttpPostParameterType.Field:
                        {
                            var fieldLine = "Content-Disposition: form-data; name=\"{0}\"".FormatWith(parameter.Name);
                            
                            written += WriteLine(write, encoding, requestStream, fieldLine);
                            written += WriteLine(write, encoding, requestStream, "");
                            written += WriteLine(write, encoding, requestStream, parameter.Value);
#if TRACE
                            if(write)
                            {
                                Trace.WriteLineIf(TraceEnabled, fieldLine);
                                Trace.WriteLineIf(TraceEnabled, "");
                                Trace.WriteLineIf(TraceEnabled, parameter.Value);
                            }
#endif
                            break;
                        }
                }
            }

            written += WriteLine(write, encoding, requestStream, footer);
#if TRACE
            if(write)
            {
                Trace.WriteLineIf(TraceEnabled, footer);
            }
#endif
            if(write)
            {
                requestStream.Flush();
                requestStream.Close();
                if (fs != null)
                {
                    fs.Dispose();
                }
            }

            return written;
        }

#if !SILVERLIGHT
        public virtual void Request(string url, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, out exception);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPut(PostOrPut.Put, url, out exception);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPut(PostOrPut.Post, url, out exception);
                    break;
                case WebMethod.Delete:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, out exception);
                    break;
                case WebMethod.Head:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, out exception);
                    break;
                case WebMethod.Options:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, out exception);
                    break;
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual void Request(string url, string key, ICache cache, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, out exception);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPut(PostOrPut.Put, url, key, cache, out exception);
                    break;
                case WebMethod.Post: 
                    ExecutePostOrPut(PostOrPut.Post, url, key, cache, out exception);
                    break;
                case WebMethod.Delete:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, out exception);
                    break;
                case WebMethod.Head:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache,  out exception);
                    break;
                case WebMethod.Options:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, out exception);
                    break;
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual void Request(string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, out exception);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPut(PostOrPut.Put, url, key, cache, absoluteExpiration, out exception);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPut(PostOrPut.Post, url, key, cache, absoluteExpiration, out exception);
                    break;
                case WebMethod.Delete:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, out exception);
                    break;
                case WebMethod.Head:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache, absoluteExpiration, out exception);
                    break;
                case WebMethod.Options:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, absoluteExpiration, out exception);
                    break;
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual void Request(string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, out exception);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPut(PostOrPut.Put, url, key, cache, slidingExpiration, out exception);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPut(PostOrPut.Post, url, key, cache, slidingExpiration, out exception);
                    break;
                case WebMethod.Delete:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, out exception);
                    break;
                case WebMethod.Head:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, out exception);
                    break;
                case WebMethod.Options:
                    ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, out exception);
                    break;
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual void Request(string url, IEnumerable<HttpPostParameter> parameters, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Put:
                    ExecutePostOrPut(PostOrPut.Put, url, parameters, out exception);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPut(PostOrPut.Post, url, parameters, out exception);
                    break;
                default:
                    throw new NotSupportedException("Only HTTP POSTs and PUTs can use multi-part parameters");
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, userState);
                default:
                    throw new NotSupportedException("Unknown web method");
            }
        }
#else
        public virtual void RequestAsync(string url, object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, userState);
                    break;
                default:
                    throw new NotSupportedException("Unknown web method");
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        string key, 
                                                        ICache cache,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url,
                                                        string key, 
                                                        ICache cache, 
                                                        DateTime absoluteExpiration,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         DateTime absoluteExpiration,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, absoluteExpiration, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        string key, 
                                                        ICache cache, 
                                                        TimeSpan slidingExpiration,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, slidingExpiration, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, slidingExpiration, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         TimeSpan slidingExpiration,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        IEnumerable<HttpPostParameter> parameters,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, parameters, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, parameters, userState);
                default:
                    throw new NotSupportedException("Only HTTP POSTS can use multi-part forms");
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         IEnumerable<HttpPostParameter> parameters,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, parameters, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, parameters, userState);
                    break;
                default:
                    throw new NotSupportedException("Only HTTP POSTS can use multi-part forms");
            }
        }
#endif

#if !SILVERLIGHT
        public virtual void ExecutePostOrPut(PostOrPut method, 
                                               string url, 
                                               string key, 
                                               ICache cache, 
                                               out WebException exception)
        {
            WebException ex = null; 
            ExecuteWithCache(cache, url, key, (c, u) => ExecutePostOrPut(method, cache, url, key, out ex));
            exception = ex;
        }

        public virtual void ExecutePostOrPut(PostOrPut method, string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            WebException ex = null; 
            ExecuteWithCacheAndAbsoluteExpiration(cache, url, key, absoluteExpiration,
                                                            (c, u, e) =>
                                                            ExecutePostOrPut(method, cache, url, key, absoluteExpiration, out ex));
            exception = ex;
        }

        public virtual void ExecutePostOrPut(PostOrPut method, string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception)
        {
            WebException ex = null; 
            ExecuteWithCacheAndSlidingExpiration(cache, url, key, slidingExpiration,
                                                           (c, u, e) =>
                                                           ExecutePostOrPut(method, cache, url, key, slidingExpiration, out ex));
            exception = ex;
        }

        private void ExecutePostOrPut(PostOrPut method, 
                                      ICache cache, 
                                      string url, 
                                      string key, 
                                      out WebException exception)
        {
            ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream);
            }
        }

        private void ExecutePostOrPut(PostOrPut method, 
                                        ICache cache, 
                                        string url, 
                                        string key,
                                        DateTime absoluteExpiration, 
                                        out WebException exception)
        {
            ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream, absoluteExpiration);
            }
        }

        private void ExecutePostOrPut(PostOrPut method, ICache cache, string url, string key,
                                        TimeSpan slidingExpiration, out WebException exception)
        {
            ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), ContentStream, slidingExpiration);
            }
        }
#endif

        public void Dispose()
        {
            if(ContentStream != null)
            {
                ContentStream.Dispose();
            }
        }
    }

    internal class PostProgressEventArgs : EventArgs
    {
        public virtual string FileName { get; set; }
        public virtual long BytesWritten { get; set; }
        public virtual long TotalBytes { get; set; }
    }
}