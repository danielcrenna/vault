using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Web.Mocks;
#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif
namespace Hammock.Web
{
    public partial class WebQuery
    {
        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method, string url, object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var state = new Triplet<WebRequest, object, object>
                            {
                                First = request,
                                Second = null,
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<Stream>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                {
                    CompletedSynchronously = true
                };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, string>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, string>
                                                 {
                                        First = cache,
                                        Second = key
                                    },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url, 
                                                            DateTime absoluteExpiration, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<Stream>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                                 {
                                     CompletedSynchronously = true,
                                     AsyncState = this
                                 };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, Pair<string, DateTime>>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, Pair<string, DateTime>>
                                                 {
                                                     First = cache,
                                                     Second = new Pair<string, DateTime>
                                                                  {
                                                                      First = key,
                                                                      Second = absoluteExpiration
                                                                  }
                                                 },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url,
                                                            TimeSpan slidingExpiration, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<Stream>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                {
                    CompletedSynchronously = true
                };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, Pair<string, TimeSpan>>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, Pair<string, TimeSpan>>
                                                 {
                                                     First = cache,
                                                     Second = new Pair<string, TimeSpan>
                                                                  {
                                                                      First = key,
                                                                      Second = slidingExpiration
                                                                  }
                                                 },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        // [DC] This is necessary to inform the client handle that a POST has timed out
        private readonly List<WebQueryAsyncResult> _postHandles = new List<WebQueryAsyncResult>();

        protected virtual void RegisterAbortTimer(WebRequest request, IAsyncResult result)
        {
            if(request is MockHttpWebRequest)
            {
                return;
            }


            var timeout = RequestTimeout != null ?
                (int)RequestTimeout.Value.TotalMilliseconds
                : 300000; // Default ReadWriteTimeout


#if WindowsPhone
            // [DS] timout handling not supported in Windows Phone so implementing
            //      with timer.
            var state = new Pair<WebRequest, IAsyncResult>
            {
                First = request,
                Second = result
            };
       
            if(this.timer==null)
                this.timer = new Timer(TimerTimedOut, state, timeout, Timeout.Infinite);
#endif

#if !Smartphone && !WindowsPhone && !SL4 && !NETCF
            // [DC] request.Timeout is ignored with async
            
            var isPost = result is WebQueryAsyncResult;
            if (isPost)
            {
                _postHandles.Add((WebQueryAsyncResult)result);
            }
            var handle = isPost
                             ? ((WebQueryAsyncResult) result).InnerResult.AsyncWaitHandle
                             : result.AsyncWaitHandle;

            var state = new Pair<WebRequest, IAsyncResult>
                            {
                                First = request,
                                Second = result
                            };
            
            // Async operations ignore the WebRequest's Timeout property
            ThreadPool.RegisterWaitForSingleObject(handle,
                                                   TimedOutCallback,
                                                   state,
                                                   timeout, 
                                                   true /* executeOnlyOnce */);
#endif
        }

#if WindowsPhone
        // [DS] Handler for Windows Phone timeouts.
        int completed = 0;
        System.Threading.Timer timer = null;

        private void TimerTimedOut(object state)
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.timer.Dispose();
            TimedOutCallback(state, true);
        }
#endif

        private void TimedOutCallback(object state, bool timedOut)
        {
            
            if (!timedOut)
            {
                return;
            }

            var pair = state as Pair<WebRequest, IAsyncResult>;
            if (pair != null)
            {
                var request = pair.First;
                var result = pair.Second;

#if WindowsPhone
                // Since this timeout is raised by a timer, check that
                // reponse has not been completed yet..
                if (Interlocked.Increment(ref completed) != 1)
                {
                    // We have finished so this is not actually a timeout
                    return;
                }
#endif
                TimedOut = true;
                request.Abort();

                // [DC] LSP violation necessary for POST functionality;
                // [DC] We did not get far enough along to prepare a response
                if (result is WebQueryAsyncResult)
                {
                    var response = new RestResponse
                    {
                        TimedOut = true,
                        StatusCode = 0
                    };
#if TRACE
                    // Just for cosmetic purposes
                    Trace.WriteLineIf(TraceEnabled, string.Concat("RESPONSE: ", response.StatusCode));
                    Trace.WriteLineIf(TraceEnabled, "\r\n");
#endif
                    foreach(var postHandle in _postHandles)
                    {
                        postHandle.AsyncState = response;
                        postHandle.Signal();
                    }
                    _postHandles.Clear();
                }
            }
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method,
                                                                      string url, 
                                                                      string prefixKey, 
                                                                      ICache cache,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, request, userState);
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method, 
                                                                      string url, 
                                                                      string prefixKey, 
                                                                      ICache cache, 
                                                                      DateTime absoluteExpiration,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, absoluteExpiration, request, userState);
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method,
                                                                      string url, 
                                                                      string prefixKey,
                                                                      ICache cache,
                                                                      TimeSpan slidingExpiration,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, slidingExpiration, request, userState);
        }
        
        protected virtual void GetAsyncResponseCallback(IAsyncResult asyncResult)
        {

#if WindowsPhone
            Interlocked.Increment(ref completed);
#endif

            Console.Out.WriteLine("----- WebQuery.Async.cs GetAsyncResponseCallback");

            object store;
            var request = GetAsyncCacheStore(asyncResult, out store);

            try
            {
                var response = request.EndGetResponse(asyncResult);
                using (response)
                {
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
                    
                    ContentStream = response.GetResponseStream();
                    
                    if (store != null)
                    {
                        // No expiration specified
                        if (store is Pair<ICache, string>)
                        {
                            var cache = store as Pair<ICache, string>;
                            cache.First.Insert(cache.Second, ContentStream);
                        }

                        // Absolute expiration specified
                        if (store is Pair<ICache, Pair<string, DateTime>>)
                        {
                            var cache = store as Pair<ICache, Pair<string, DateTime>>;
                            cache.First.Insert(cache.Second.First, ContentStream, cache.Second.Second);
                        }

                        // Sliding expiration specified
                        if (store is Pair<ICache, Pair<string, TimeSpan>>)
                        {
                            var cache = store as Pair<ICache, Pair<string, TimeSpan>>;
                            cache.First.Insert(cache.Second.First, ContentStream, cache.Second.Second);
                        }
                    }

                    // Only send query when caching is complete
                    var args = new WebQueryResponseEventArgs(ContentStream);
                    OnQueryResponse(args);
                }
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        private static WebRequest GetAsyncCacheStore(IAsyncResult asyncResult, out object store)
        {
            /*
            var state = asyncResult.AsyncState as Triplet<WebRequest, Triplet<ICache, object, string>, object>;
            if (state == null)
            {
                throw new ArgumentNullException("asyncResult", 
                                                "The asynchronous post failed to return its state");
            }

            var request = state.First;
            if (request == null)
            {
                throw new ArgumentNullException("asyncResult", 
                                                "The asynchronous post failed to return a request");
            }
             */


            WebRequest request;

            var noCache = asyncResult.AsyncState as Triplet<WebRequest, object, object>;
            if(noCache != null)
            {
                request = noCache.First;
                store = noCache.Second;
            }
            else
            {
                var absoluteCache = asyncResult.AsyncState as Triplet<WebRequest, Pair<ICache, Pair<string, DateTime>>, object>;
                if(absoluteCache != null)
                {
                    request = absoluteCache.First;
                    store = absoluteCache.Second;
                }
                else
                {
                    var slidingCache = asyncResult.AsyncState as Triplet<WebRequest, Pair<ICache, Pair<string, TimeSpan>>, object>;
                    if(slidingCache != null)
                    {
                        request = slidingCache.First;
                        store = slidingCache.Second;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            "asyncResult", "Wrong cache signature found."
                            );
                    }
                }
            }
            return request;
        }

        private bool _isStreaming;
        private readonly byte[] _endStreamBytes = Encoding.UTF8.GetBytes("END STREAMING");

        public virtual bool IsStreaming
        {
            get
            {
                lock(_sync)
                {
                    return _isStreaming;
                }
            }
            set
            {
                lock(_sync)
                {
                    _isStreaming = value;
                }
            }
        }

        public virtual bool TimedOut { get; set; }

        private void AsyncStreamCallback(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as Pair<WebRequest, Pair<TimeSpan, int>>;
            if (state == null)
            {
                // Unrecognized state signature
                throw new ArgumentNullException("asyncResult",
                                                "The asynchronous post failed to return its state");
            }

            var request = state.First;
            var duration = state.Second.First;
            var resultCount = state.Second.Second;

            WebResponse response = null;
            Stream stream = null;

            try
            {
                using (response = request.EndGetResponse(asyncResult))
                {
#if SILVERLIGHT
                    if (DecompressionMethods == Silverlight.Compat.DecompressionMethods.GZip ||
                        DecompressionMethods == Silverlight.Compat.DecompressionMethods.Deflate ||
                        DecompressionMethods == (Silverlight.Compat.DecompressionMethods.GZip | Silverlight.Compat.DecompressionMethods.Deflate)
                        )
                    {
                        response = new GzipHttpWebResponse((HttpWebResponse)response);
                    }
#endif
                    StreamImpl(out stream, request, response);
                }
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }

                WebResponse = response;
            }
        }

        public delegate void NewStreamMessage(Stream message);
        public event NewStreamMessage NewStreamMessageEvent;

        private void StreamImpl(out Stream stream, WebRequest request, WebResponse response)
        {
            using (stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    throw new Exception("No stream returned from streaming request");
                }

                NewStreamMessageEvent += WebQueryNewStreamMessageEvent;

                _isStreaming = true;

                try
                {
                    int byteAsInt = 0;
                    var messageBuilder = new StringBuilder();
                    var decoder = Encoding.UTF8.GetDecoder();
                    var nextChar = new char[1];

                    while ((byteAsInt = stream.ReadByte()) != -1)
                    {
                        var charCount = decoder.GetChars(new[] {(byte) byteAsInt}, 0, 1, nextChar, 0);
                        if(charCount == 0) continue;

                        messageBuilder.Append(nextChar);

                        if (nextChar[0] == streamResultDelimiter)
                        {
                            ProcessBuffer(messageBuilder.ToString());
#if NET40
                            messageBuilder.Clear();
#else
                            messageBuilder = new StringBuilder(); // No .Clear() method
#endif
                        }
                    }
                }
                finally
                {
                    EndStreaming(request);
                }
            }
        }

        private void WebQueryNewStreamMessageEvent(Stream message)
        {
            var args = new WebQueryResponseEventArgs(message);
            OnQueryResponse(args);
        }

        // TODO make part of StreamOptions
        private const char streamResultDelimiter = '\r';

        private void ProcessBuffer(string bufferString)
        {
            var buffer = bufferString;
            var position = buffer.IndexOf(streamResultDelimiter);

            while (position >= 0)
            {
                string message = buffer.Substring(0, position).Replace(streamResultDelimiter.ToString(),"");

                var messageBytes = Encoding.UTF8.GetBytes(message);

                buffer = buffer.Length <= position + 1 ?
                    string.Empty : buffer.Substring(position + 1);

                if (message.Trim().Length > 1 && NewStreamMessageEvent != null)
                {
                    NewStreamMessageEvent(new MemoryStream(messageBytes));
                }

                position = buffer.IndexOf(streamResultDelimiter);
            }
        }

        private void EndStreaming(WebRequest request)
        {
            _isStreaming = false;
            
            var stream = new MemoryStream(_endStreamBytes);
            var args = new WebQueryResponseEventArgs(stream);
            OnQueryResponse(args);
            request.Abort();
        }

        protected virtual void PostAsyncStreamRequestCallback(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as Triplet<WebRequest, byte[], Pair<TimeSpan, int>>;
            if (state == null)
            {
                // Unrecognized state signature
                throw new ArgumentNullException("asyncResult",
                                                "The asynchronous post failed to return its state");
            }

            var request = state.First;
            var content = state.Second;

            using (var stream = request.EndGetRequestStream(asyncResult))
            {
                if (content != null)
                {
                    stream.Write(content, 0, content.Length);
                    stream.Flush();
                }
                stream.Close();

                request.BeginGetResponse(AsyncStreamCallback,
                                         new Pair<WebRequest, Pair<TimeSpan, int>>
                                             {
                                                 First = request,
                                                 Second = state.Third
                                             }
                    );
            }
        }

        protected virtual void PostAsyncRequestCallback(IAsyncResult asyncResult)
        {
            WebRequest request;
            byte[] post;
            object userState;
            Triplet<ICache, object, string> store;

            var state = asyncResult.AsyncState as Triplet<WebRequest, byte[], object>;
            if (state == null)
            {
                // No expiration specified
                if (asyncResult is Triplet<WebRequest, Triplet<byte[], ICache, string>, object>)
                {
                    var cacheScheme = (Triplet<WebRequest, Triplet<byte[], ICache, string>, object>)asyncResult;
                    var cache = cacheScheme.Second.Second;

                    var url = cacheScheme.First.RequestUri.ToString();
                    var prefix = cacheScheme.Second.Third;
                    var key = CreateCacheKey(prefix, url);

                    var fetch = cache.Get<Stream>(key);
                    if (fetch != null)
                    {
                        var args = new WebQueryResponseEventArgs(fetch);
                        OnQueryResponse(args);
                        return;
                    }

                    request = cacheScheme.First;
                    post = cacheScheme.Second.First;
                    userState = cacheScheme.Third;
                    store = new Triplet<ICache, object, string>
                    {
                        First = cache,
                        Second = null,
                        Third = prefix
                    };
                }
                else
                    // Absolute expiration specified
                    if (asyncResult is Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>)
                    {
                        var cacheScheme = (Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>)asyncResult;
                        var url = cacheScheme.First.RequestUri.ToString();
                        var cache = cacheScheme.Second.Second.First;
                        var expiry = cacheScheme.Second.Second.Second;

                        var prefix = cacheScheme.Second.Second.Third;
                        var key = CreateCacheKey(prefix, url);

                        var fetch = cache.Get<Stream>(key);
                        if (fetch != null)
                        {
                            var args = new WebQueryResponseEventArgs(fetch);
                            OnQueryResponse(args);
                            return;
                        }

                        request = cacheScheme.First;
                        post = cacheScheme.Second.First;
                        userState = cacheScheme.Third;
                        store = new Triplet<ICache, object, string>
                        {
                            First = cache,
                            Second = expiry,
                            Third = prefix
                        };
                    }
                    else
                        // Sliding expiration specified
                        if (asyncResult is Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>)
                        {
                            var cacheScheme = (Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>)asyncResult;
                            var url = cacheScheme.First.RequestUri.ToString();
                            var cache = cacheScheme.Second.Second.First;
                            var expiry = cacheScheme.Second.Second.Second;

                            var prefix = cacheScheme.Second.Second.Third;
                            var key = CreateCacheKey(prefix, url);

                            var fetch = cache.Get<Stream>(key);
                            if (fetch != null)
                            {
                                var args = new WebQueryResponseEventArgs(fetch);
                                OnQueryResponse(args);
                                return;
                            }

                            request = cacheScheme.First;
                            post = cacheScheme.Second.First;
                            userState = cacheScheme.Third;
                            store = new Triplet<ICache, object, string>
                            {
                                First = cache,
                                Second = expiry,
                                Third = prefix
                            };
                        }
                        else
                        {
                            // Unrecognized state signature
                            throw new ArgumentNullException("asyncResult",
                                                            "The asynchronous post failed to return its state");
                        }
            }
            else
            {
                request = state.First;
                post = state.Second;
                userState = state.Third;
                store = null;
            }

            // No cached response
            using (var stream = request.EndGetRequestStream(asyncResult))
            {
                WritePostContentToRequestStream(post, stream);

                var inner = request.BeginGetResponse(PostAsyncResponseCallback,
                                         new Triplet<WebRequest, Triplet<ICache, object, string>, object>
                                         {
                                             First = request,
                                             Second = store,
                                             Third = userState
                                         });

                RegisterAbortTimer(request, new WebQueryAsyncResult { InnerResult = inner });
            }
        }

        private void WritePostContentToRequestStream(byte[] post, Stream stream)
        {
            if (post != null)
            {
                stream.Write(post, 0, post.Length);
                stream.Flush();
            }
            stream.Close();
#if TRACE
            var encoding = Encoding ?? new UTF8Encoding();
            if (post != null)
            {
                Trace.WriteLineIf(TraceEnabled, encoding.GetString(post, 0, post.Length));
            }
#endif
        }

        protected virtual void PostAsyncRequestCallbackMultiPart(IAsyncResult asyncResult)
        {
            WebRequest request;
            string boundary;
            IEnumerable<HttpPostParameter> parameters;
            object userState;
            Triplet<ICache, object, string> store;

            var state = asyncResult.AsyncState as Triplet<WebRequest, Pair<string, IEnumerable<HttpPostParameter>>, object>;
            if (state == null)
            {
                // No expiration specified
                if (asyncResult is Triplet<WebRequest, Triplet<Pair<string, IEnumerable<HttpPostParameter>>, ICache, string>, object>)
                {
                    #region No Expiration
                    var cacheScheme = (Triplet<WebRequest, Triplet<Pair<string, IEnumerable<HttpPostParameter>>, ICache, string>, object>)asyncResult;
                    var cache = cacheScheme.Second.Second;

                    var url = cacheScheme.First.RequestUri.ToString();
                    var prefix = cacheScheme.Second.Third;
                    var key = CreateCacheKey(prefix, url);

                    var fetch = cache.Get<Stream>(key);
                    if (fetch != null)
                    {
                        var args = new WebQueryResponseEventArgs(fetch);
                        OnQueryResponse(args);
                        return;
                    }

                    request = cacheScheme.First;
                    boundary = cacheScheme.Second.First.First;
                    parameters = cacheScheme.Second.First.Second;
                    userState = cacheScheme.Third;
                    store = new Triplet<ICache, object, string>
                                {
                                    First = cache,
                                    Second = null,
                                    Third = prefix
                                };
                    #endregion
                }
                else
                    // Absolute expiration specified
                    if (asyncResult is Triplet<WebRequest, Pair<Pair<string, IEnumerable<HttpPostParameter>>, Triplet<ICache, DateTime, string>>, object>)
                    {
                        #region Absolute Expiration
                        var cacheScheme = (Triplet<WebRequest, Pair<Pair<string, IEnumerable<HttpPostParameter>>, Triplet<ICache, DateTime, string>>, object>)asyncResult;
                        var url = cacheScheme.First.RequestUri.ToString();
                        var cache = cacheScheme.Second.Second.First;
                        var expiry = cacheScheme.Second.Second.Second;

                        var prefix = cacheScheme.Second.Second.Third;
                        var key = CreateCacheKey(prefix, url);

                        var fetch = cache.Get<Stream>(key);
                        if (fetch != null)
                        {
                            var args = new WebQueryResponseEventArgs(fetch);
                            OnQueryResponse(args);
                            return;
                        }

                        request = cacheScheme.First;
                        boundary = cacheScheme.Second.First.First;
                        parameters = cacheScheme.Second.First.Second;
                        userState = cacheScheme.Third;
                        store = new Triplet<ICache, object, string>
                                    {
                                        First = cache,
                                        Second = expiry,
                                        Third = prefix
                                    };
                        #endregion
                    }
                    else
                        // Sliding expiration specified
                        if (asyncResult is Triplet<WebRequest, Pair<Pair<string, IEnumerable<HttpPostParameter>>, Triplet<ICache, TimeSpan, string>>, object>)
                        {
                            #region Sliding Expiration
                            var cacheScheme = (Triplet<WebRequest, Pair<Pair<string, IEnumerable<HttpPostParameter>>, Triplet<ICache, TimeSpan, string>>, object>)asyncResult;
                            var url = cacheScheme.First.RequestUri.ToString();
                            var cache = cacheScheme.Second.Second.First;
                            var expiry = cacheScheme.Second.Second.Second;

                            var prefix = cacheScheme.Second.Second.Third;
                            var key = CreateCacheKey(prefix, url);

                            var fetch = cache.Get<Stream>(key);
                            if (fetch != null)
                            {
                                var args = new WebQueryResponseEventArgs(fetch);
                                OnQueryResponse(args);
                                return;
                            }

                            request = cacheScheme.First;
                            boundary = cacheScheme.Second.First.First;
                            parameters = cacheScheme.Second.First.Second;
                            userState = cacheScheme.Third;
                            store = new Triplet<ICache, object, string>
                                        {
                                            First = cache,
                                            Second = expiry,
                                            Third = prefix
                                        };
                            #endregion
                        }
                        else
                        {
                            // Unrecognized state signature
                            throw new ArgumentNullException("asyncResult",
                                                            "The asynchronous post failed to return its state");
                        }
            }
            else
            {
                request = state.First;
                boundary = state.Second.First;
                parameters = state.Second.Second;
                userState = state.Third;
                store = null;
            }

#if !Smartphone
            var encoding = Encoding ?? Encoding.GetEncoding("ISO-8859-1");
#else
            var encoding = Encoding ?? Encoding.GetEncoding(1252);
#endif
            var expected = WriteMultiPartImpl(
                false /* write */, parameters, boundary, encoding, null
                );
            
            // No cached response
            using (var requestStream = request.EndGetRequestStream(asyncResult))
            {
                var actual = WriteMultiPartImpl(
                        true /* write */, parameters, boundary, encoding, requestStream
                        );

                Debug.Assert(expected == actual, string.Format("Expected {0} bytes but wrote {1}!", expected, actual));

                var inner = request.BeginGetResponse(PostAsyncResponseCallback,
                                         new Triplet<WebRequest, Triplet<ICache, object, string>, object>
                                             {
                                                 First = request,
                                                 Second = store,
                                                 Third = userState
                                             });

                RegisterAbortTimer(request, new WebQueryAsyncResult { InnerResult = inner });
            }
        }

        protected virtual void PostAsyncResponseCallback(IAsyncResult asyncResult)
        {
#if WindowsPhone
            Interlocked.Increment(ref completed);
#endif

            WebRequest request;
            Triplet<WebRequest, Triplet<ICache, object, string>, object> state;
            try
            {
                state = asyncResult.AsyncState as Triplet<WebRequest, Triplet<ICache, object, string>, object>;
                if (state == null)
                {
                    throw new ArgumentNullException("asyncResult", "The asynchronous post failed to return its state");
                }

                request = state.First;
                if (request == null)
                {
                    throw new ArgumentNullException("asyncResult", "The asynchronous post failed to return a request");
                }
            }
            catch(Exception ex)
            {
                var args = new WebQueryResponseEventArgs(new MemoryStream(), ex);
                OnQueryResponse(args);
                return;
            }

            try
            {
                // Avoid disposing until no longer needed to build results
                var response = request.EndGetResponse(asyncResult);

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

                ContentStream = response.GetResponseStream();
                if (state.Second != null)
                {
                    var cache = state.Second.First;
                    var expiry = state.Second.Second;
                    var url = request.RequestUri.ToString();

                    var prefix = state.Second.Third;
                    var key = CreateCacheKey(prefix, url);

                    if (expiry is DateTime)
                    {
                        // absolute
                        cache.Insert(key, ContentStream, (DateTime)expiry);
                    }

                    if (expiry is TimeSpan)
                    {
                        // sliding
                        cache.Insert(key, ContentStream, (TimeSpan)expiry);
                    }
                }

                var args = new WebQueryResponseEventArgs(ContentStream);
                OnQueryResponse(args);
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, string url, object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, byte[], object>
                            {
                                First = request,
                                Second = content,
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            RegisterAbortTimer(request, result);
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method,
                                                                    string url,
                                                                    IEnumerable<HttpPostParameter> parameters,
                                                                    object userState)
        {
            WebResponse = null;

            string boundary;
            var request = BuildMultiPartFormRequest(method, url, parameters, out boundary);

            var state = new Triplet<WebRequest, Pair<string, IEnumerable<HttpPostParameter>>, object>
                            {
                                First = request,
                                Second = new Pair<string, IEnumerable<HttpPostParameter>>
                                             {
                                                 First = boundary,
                                                 Second = parameters
                                             },
                                Third = userState
                            };
            var args = new WebQueryRequestEventArgs(url);

            OnQueryRequest(args);
            
            var inner = request.BeginGetRequestStream(PostAsyncRequestCallbackMultiPart, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            RegisterAbortTimer(request, result);
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey,
                                                                    ICache cache,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Triplet<byte[], ICache, string>, object>
                            {
                                First = request,
                                Second = new Triplet<byte[], ICache, string>
                                             {
                                                 First = content,
                                                 Second = cache,
                                                 Third = prefixKey
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            RegisterAbortTimer(request, result);
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey, 
                                                                    ICache cache, 
                                                                    DateTime absoluteExpiration,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>
                            {
                                First = request,
                                Second = new Pair<byte[], Triplet<ICache, DateTime, string>>
                                             {
                                                 First = content,
                                                 Second = new Triplet<ICache, DateTime, string>
                                                              {
                                                                  First = cache,
                                                                  Second = absoluteExpiration,
                                                                  Third = prefixKey
                                                              }
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            RegisterAbortTimer(request, result);
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey,
                                                                    ICache cache, 
                                                                    TimeSpan slidingExpiration,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>
                            {
                                First = request,
                                Second = new Pair<byte[], Triplet<ICache, TimeSpan, string>>
                                             {
                                                 First = content,
                                                 Second = new Triplet<ICache, TimeSpan, string>
                                                              {
                                                                  First = cache,
                                                                  Second = slidingExpiration,
                                                                  Third = prefixKey
                                                              }
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            RegisterAbortTimer(request, result);
            return result;
        }

        public virtual WebQueryAsyncResult ExecuteStreamGetAsync(string url, 
                                                                 TimeSpan duration, 
                                                                 int resultCount)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(GetDeleteHeadOptions.Get, url);
            var state = OnGetStreamQueryRequest(url, request, duration, resultCount);

#if SILVERLIGHT
            var httpRequest = request as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.AllowReadStreamBuffering = false;
            }
#endif

            var inner = request.BeginGetResponse(AsyncStreamCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        private Pair<WebRequest, Pair<TimeSpan, int>> OnGetStreamQueryRequest(string url, WebRequest request, TimeSpan duration, int resultCount)
        {
            var state = new Pair<WebRequest, Pair<TimeSpan, int>>
                            {
                                First = request,
                                Second = new Pair<TimeSpan, int>
                                             {
                                                 First = duration,
                                                 Second = resultCount
                                             }
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);
            return state;
        }

        private Triplet<WebRequest, byte[], Pair<TimeSpan, int>> OnPostStreamQueryRequest(string url, WebRequest request, byte[] content, TimeSpan duration, int resultCount)
        {
            var state = new Triplet<WebRequest, byte[], Pair<TimeSpan, int>>
            {
                First = request,
                Second = content,
                Third = new Pair<TimeSpan, int>
                {
                    First = duration,
                    Second = resultCount
                }
            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);
            return state;
        }

        public virtual WebQueryAsyncResult ExecuteStreamPostAsync(string url, 
                                                                  TimeSpan duration, 
                                                                  int resultCount)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(PostOrPut.Post, url, out content);
            var state = OnPostStreamQueryRequest(url, request, content, duration, resultCount);

#if SILVERLIGHT
            var httpRequest = request as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.AllowReadStreamBuffering = false;
            }
#endif

            var inner = request.BeginGetRequestStream(PostAsyncStreamRequestCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        private object ResponseAsHttpWebResponse(out string version, 
                                                 out int statusCode, 
                                                 out string statusDescription, 
                                                 out string contentType, 
                                                 out long contentLength, 
                                                 out Uri responseUri, 
                                                 out System.Net.WebHeaderCollection headers)
        {
            var httpWebResponse = WebResponse != null &&
                                  (WebResponse is HttpWebResponse)
                                      ? (HttpWebResponse) WebResponse
                                      : null;

            if(httpWebResponse == null)
            {
                version = null;
                statusCode = 0;
                statusDescription = null;
                contentType = null;
                contentLength = 0;
                responseUri = null;
                headers = null;
                return null;
            }

#if !SILVERLIGHT
            version = string.Concat("HTTP/", httpWebResponse.ProtocolVersion);
#else
            version = "HTTP/1.1";
#endif
            statusCode = Convert.ToInt32(httpWebResponse.StatusCode, CultureInfo.InvariantCulture);
            statusDescription = httpWebResponse.StatusDescription;
            contentType = httpWebResponse.ContentType;
            contentLength = httpWebResponse.ContentLength;
            responseUri = httpWebResponse.ResponseUri;
            headers = httpWebResponse.Headers;
            return httpWebResponse;
        }

        private object ResponseAsMockHttpWebResponse(out int statusCode, 
                                                     out string statusDescription, 
                                                     out string contentType, 
                                                     out long contentLength, 
                                                     out Uri responseUri, 
                                                     out System.Net.WebHeaderCollection headers)
        {
            var httpWebResponse = WebResponse != null && WebResponse is MockHttpWebResponse
                                      ? (MockHttpWebResponse)WebResponse
                                      : null;

            if (httpWebResponse == null)
            {
                statusCode = 0;
                statusDescription = null;
                contentType = null;
                contentLength = 0;
                responseUri = null;
                headers = null;
                return null;
            }

            statusCode = Convert.ToInt32(httpWebResponse.StatusCode, CultureInfo.InvariantCulture);
            statusDescription = httpWebResponse.StatusDescription;
            contentType = httpWebResponse.ContentType;
            contentLength = httpWebResponse.ContentLength;
            responseUri = httpWebResponse.ResponseUri;
            headers = httpWebResponse.Headers;
            return httpWebResponse;
        }

        private void CastWebResponse(out string version,
                                     out int statusCode, 
                                     out string statusDescription, 
                                     out System.Net.WebHeaderCollection headers,
                                     out string contentType, 
                                     out long contentLength, 
                                     out Uri responseUri)
        {
            var response = ResponseAsHttpWebResponse(
                out version, out statusCode, out statusDescription, 
                out contentType, out contentLength, 
                out responseUri, out headers
                );
            if (response != null)
            {
                return;
            }

            response = ResponseAsMockHttpWebResponse(
                out statusCode, out statusDescription,
                out contentType, out contentLength,
                out responseUri, out headers
                );

            // [DC]: Caching would result in a null response
            if(response == null)
            {
               headers = new System.Net.WebHeaderCollection(); 
            }
        }
    }
}