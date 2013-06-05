using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace hammock2
{
    public partial class Http
    {
        static Http()
        {
            Engine = new HttpClientEngine();
        }
    }
    public class HttpClientEngine : IHttpEngine
    {
        public static Func<HttpClient> ClientFactory = () => PerRequestHandler != null ? new HttpClient(PerRequestHandler, false) : new HttpClient(DefaultHandler, false);
        public static HttpClientHandler DefaultHandler = new HttpClientHandler
        {
            PreAuthenticate = true,
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None
        };
        public static HttpClientHandler PerRequestHandler = DefaultHandler;
        public static HttpClientHandler NtlmHandler = new HttpClientHandler
        {
            UseDefaultCredentials = true,
            PreAuthenticate = true,
            ClientCertificateOptions = ClientCertificateOption.Automatic
        };

        private HttpClient _client;

        public dynamic Request(string url, string method, NameValueCollection headers, dynamic body, bool trace)
        {
            var request = BuildRequest(url, method, headers, body);
            if (trace) TraceRequest(request);
            var reply = BuildResponse(request, url, method);
            if (trace) TraceResponse(reply.Response);
            return reply;
        }

        private static HttpRequestMessage BuildRequest(string url, string method, NameValueCollection headers, dynamic body)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(url) };
            foreach (var name in headers.AllKeys)
            {
                var value = headers[name];
                request.Headers.Add(name, value);
            }
            if (string.IsNullOrEmpty(request.Headers.UserAgent.ToString()))
            {
                request.Headers.Add("User-Agent", "hammock2");
            }
            switch (method.Trim().ToUpperInvariant())
            {
                case "GET":
                    request.Method = HttpMethod.Get;
                    break;
                case "POST":
                    request.Method = HttpMethod.Post;
                    break;
                case "PUT":
                    request.Method = HttpMethod.Put;
                    break;
                case "DELETE":
                    request.Method = HttpMethod.Delete;
                    break;
                case "HEAD":
                    request.Method = HttpMethod.Head;
                    break;
                case "OPTIONS":
                    request.Method = HttpMethod.Options;
                    break;
                case "TRACE":
                    request.Method = HttpMethod.Trace;
                    break;
            }

            // Content negotiation goes here...
            HttpContent content = null;
            if (body != null)
            {
                content = new ObjectContent(body.GetType(), body, new JsonMediaTypeFormatter());
            }
            request.Content = content;
            return request;
        }

        public dynamic BuildResponse(HttpRequestMessage request, string url, string method)
        {
            _client = _client ?? ClientFactory();
            var response = _client.SendAsync(request).Result;

            // Content negotiation goes here...
            var bodyString = response.Content != null ? response.Content.ReadAsStringAsync().Result : null;
            HttpBody body = bodyString != null ? HttpBody.Deserialize(bodyString) : null;
            return new HttpReply
            {
                Body = body,
                Response = response
            };
        }

        private static void TraceRequest(HttpRequestMessage request)
        {
            TraceRequestImpl(request);
        }
        private static void TraceResponse(HttpResponseMessage response)
        {
            TraceResponseImpl(response);
        }
        [Conditional("TRACE")]
        private static void TraceRequestImpl(HttpRequestMessage request)
        {
            Trace.WriteLine(string.Concat("--REQUEST: ", request.RequestUri.Scheme, "://", request.RequestUri.Host));
            var pathAndQuery = string.Concat(request.RequestUri.AbsolutePath, string.IsNullOrEmpty(request.RequestUri.Query) ? "" : string.Concat(request.RequestUri.Query));
            Trace.WriteLine(string.Concat(request.Method, " ", pathAndQuery, " HTTP/", request.Version));
            foreach (var header in request.Headers)
            {
                Trace.Write(header.Key);
                Trace.Write(": ");
                var count = header.Value.Count();
                var i = 0;
                foreach (var value in header.Value)
                {
                    Trace.Write(value);
                    i++;
                    Trace.WriteIf(count < i, ",");
                }
                Trace.WriteLine("");
            }
            if (request.Content != null)
            {
                Trace.WriteLine(request.Content.ReadAsStringAsync().Result);
            }
        }
        [Conditional("TRACE")]
        private static void TraceResponseImpl(HttpResponseMessage response)
        {
            Trace.WriteLine(String.Concat("\r\n--RESPONSE:", " ", response.RequestMessage.RequestUri));
            Trace.WriteLine(String.Concat("HTTP/", response.RequestMessage.Version, " ", (int)response.StatusCode, " ", response.ReasonPhrase));
            foreach (var header in response.Headers)
            {
                Trace.Write(header.Key);
                Trace.Write(": ");
                var count = header.Value.Count();
                var i = 0;
                foreach (var value in header.Value)
                {
                    Trace.Write(value);
                    i++;
                    Trace.WriteIf(count < i, ",");
                }
                Trace.WriteLine("");
            }
            if (response.Content != null)
            {
                Trace.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}