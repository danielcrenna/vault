using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using cohort.API.Formatters;

namespace cohort.Tests.Api
{
    public static class HttpClientExtensions
    {
        public static HttpResponseMessage Get(this HttpClient client, string baseAddress, string relativeUrl)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(new Uri(baseAddress), relativeUrl) };
            return Get(client, request);
        }

        public static HttpResponseMessage Get(this HttpClient client, string url)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(url) };
            return Get(client, request);
        }

        public static HttpResponseMessage Get(this HttpClient client, string baseAddress, string relativeUrl, Action<HttpRequestMessage> replay)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(new Uri(baseAddress), relativeUrl) };
            replay(request);
            return Get(client, request);
        }

        public static HttpResponseMessage Get(this HttpClient client, string url, Action<HttpRequestMessage> replay)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(url) };
            replay(request);
            return Get(client, request);
        }

        private static HttpResponseMessage Get(HttpClient client, HttpRequestMessage request)
        {
            TraceRequestImpl(client, request);
            var response = client.SendAsync(request).Result;
            TraceResponseImpl(response);
            return response;
        }

        public static HttpResponseMessage Post<T>(this HttpClient client, string url, T entity, bool trace = true, MediaTypeFormatter formatter = null)
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(url) };
            var content = new ObjectContent<T>(entity, formatter ?? new JsonFormatter());
            request.Content = content;
            request.Method = HttpMethod.Post;
            if (trace) TraceRequestImpl(client, request);
            var response = client.SendAsync(request).Result;
            if (trace) TraceResponseImpl(response);
            return response;
        }

        [Conditional("TRACE")]
        private static void TraceRequestImpl(HttpClient client, HttpRequestMessage request)
        {
            Trace.WriteLine(string.Concat("--REQUEST: ", request.RequestUri.Scheme, "://", request.RequestUri.Host));
            TraceHeaders(client.DefaultRequestHeaders);
            var pathAndQuery = string.Concat(request.RequestUri.AbsolutePath, string.IsNullOrEmpty(request.RequestUri.Query) ? "" : request.RequestUri.Query);
            Trace.WriteLine(string.Concat(request.Method, " ", pathAndQuery, " HTTP/", request.Version));
            TraceHeaders(request.Headers);
            if (request.Content == null) return;
            TraceHeaders(request.Content.Headers);
            Trace.WriteLine(request.Content.ReadAsStringAsync().Result);
        }

        [Conditional("TRACE")]
        private static void TraceResponseImpl(HttpResponseMessage response)
        {
            Trace.WriteLine(String.Concat("\r\n--RESPONSE:", " ", response.RequestMessage.RequestUri));
            Trace.WriteLine(String.Concat("HTTP/", response.RequestMessage.Version, " ", (int)response.StatusCode, " ", response.ReasonPhrase));
            TraceHeaders(response.Headers);
            if (response.Content == null) return;
            TraceHeaders(response.Content.Headers);
            Trace.WriteLine(response.Content.ReadAsStringAsync().Result);
        }

        [Conditional("TRACE")]
        private static void TraceHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                Trace.Write(header.Key);
                Trace.Write(": ");
                var count = header.Value.Count();
                var i = 0;
                foreach (var value in header.Value)
                {
                    Trace.Write(value);
                    i++;
                    Trace.WriteIf(i < count, ", ");
                }
                Trace.WriteLine("");
            }
        }
    }
}