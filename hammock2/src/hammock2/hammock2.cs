#region License
// hammock2
// --------------------------------------
// Copyright (c) 2012 Conatus Creative Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE. 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace hammock2
{
    public interface IMediaConverter
    {
        string DynamicToString(dynamic instance);
        IDictionary<string, object> StringToHash(string json);
        string HashToString(IDictionary<string, object> hash);
        T DynamicTo<T>(dynamic instance);
        T StringTo<T>(string instance);
    }

    public partial class HttpBody : DynamicObject
    {
        private readonly IDictionary<string, object> _hash = new Dictionary<string, object>();

        protected internal static readonly Null Null = new Null();
        protected internal static readonly IMediaConverter Converter;

        public static string Serialize(dynamic instance)
        {
            return Converter.DynamicToString(instance);
        }
        public static dynamic Deserialize(string content)
        {
            return new HttpBody(Converter.StringToHash(content));
        }
        public static T Deserialize<T>(dynamic instance)
        {
            return Converter.DynamicTo<T>(instance);
        }
        public T Deserialize<T>(string content)
        {
            return Converter.StringTo<T>(content);
        }
        public T Deserialize<T>()
        {
            return Converter.DynamicTo<T>(this);
        }

        public HttpBody(IEnumerable<KeyValuePair<string, object>> hash)
        {
            _hash.Clear();
            foreach (var entry in hash ?? new Dictionary<string, object>())
            {
                _hash.Add(Underscored(entry.Key), entry.Value);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var name = Underscored(binder.Name);
            _hash[name] = value;
            return _hash[name] == value;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = Underscored(binder.Name);
            if (name.Equals("null"))
            {
                result = Null;
                return true;
            }
            return YieldMember(name, out result);
        }

        public override string ToString()
        {
            return Converter.HashToString(_hash);
        }

        private bool YieldMember(string name, out object result)
        {
            object value;
            if (_hash.TryGetValue(name, out value))
            {
                var json = value.ToString();
                if (json.TrimStart(' ').StartsWith("{"))
                {
                    var nested = Converter.StringToHash(json);
                    result = new HttpBody(nested);
                    return true;
                }
                result = json;
                return _hash[name] == result;
            }
            result = Null;
            return true;
        }

        internal static string Underscored(IEnumerable<char> pascalCase)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var c in pascalCase)
            {
                if (char.IsUpper(c) && i > 0)
                {
                    sb.Append("_");
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString().ToLowerInvariant();
        }
    }

    public class Null : DynamicObject
    {
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this;
            return true;
        }
    }

    public interface IHttpEngine
    {
        dynamic Request(string url, string method, NameValueCollection headers, dynamic body, bool trace);
    }

    public partial class Http : DynamicObject
    {
        private static readonly IHttpEngine Engine;
        private UrlSegment _node;
        private readonly NameValueCollection _headers;
        private Action<Http> _auth;

        public string Endpoint { get; private set; }

        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        public Action<Http> Auth
        {
            get { return _auth; }
            set { _auth = value; }
        }

        public bool Trace { get; set; }

        public Http(string endpoint)
        {
            if (endpoint.EndsWith("/")) endpoint = endpoint.TrimEnd('/');
            Endpoint = endpoint;
            _headers = new NameValueCollection();
        }

        public dynamic Query
        {
            get { return this; }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var name = binder.Name.ToLowerInvariant();
            if (name.Equals("auth"))
            {
                if (value is Action<Http>)
                {
                    _auth = value as Action<Http>;
                }
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name.ToLowerInvariant();
            if (name.Equals("headers"))
            {
                result = _headers;
                return true;
            }
            if (name.Equals("auth"))
            {
                result = _auth;
                return true;
            }
            if (_node != null)
            {
                return _node.TryGetMember(binder, out result);
            }
            _node = new UrlSegment(this, name);
            result = _node;
            return true;
        }

        // Single segment with parameters
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name.ToLowerInvariant();
            var node = new UrlSegment(this, name);
            return node.TryInvokeMember(binder, args, out result);
        }

        // Single segment without parameters
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var argTypes = args.Select(arg => arg.GetType()).ToArray();
            if (argTypes.Length == 0 || argTypes.Length == 1 && binder.CallInfo.ArgumentNames[0].StartsWith(PrivateParameter))
            {
                var method = GetMethodOverride("GET", args, binder.CallInfo.ArgumentNames);
                result = Execute(Endpoint, "", method, null);
                return true;
            }
            var ctor = typeof(Http).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, argTypes, null);
            if (ctor == null)
            {
                result = null;
                return false;
            }
            result = ctor.Invoke(args);
            return true;
        }

        internal dynamic Execute(string url, string queryString, string method, dynamic body)
        {
            if (_auth != null)
            {
                _auth(this);
            }
            var response = Engine.Request(string.Concat(url, queryString), method, _headers, body, Trace);
            return response;
        }

        private const string PrivateParameter = "__";
        private static string GetMethodOverride(string method, IList<object> args, IList<string> names)
        {
            foreach (var name in names.Where(n => n.StartsWith(PrivateParameter)))
            {
                var parameter = name.Remove(0, 2);
                if (!parameter.Equals("method", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var index = names.IndexOf(name);
                method = args[index].ToString();
            }
            return method;
        }

        public class UrlSegment : DynamicObject
        {
            private UrlSegment _inner;
            private readonly Http _http;
            private string Name { get; set; }
            private string Separator { get; set; }

            public UrlSegment(Http client, string name)
            {
                _http = client;
                Name = name.Equals("dot") ? "" : name;
                Separator = name.Equals("dot") ? "." : "/";
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var name = binder.Name.ToLower();
                if (_inner != null)
                {
                    return _inner.TryGetMember(binder, out result);
                }
                _inner = new UrlSegment(_http, name);
                result = _inner;
                return true;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                // Resolve the node if not already
                if (_http._node != null)
                {
                    var name = binder.Name.ToLower();
                    _inner = new UrlSegment(_http, name);
                }

                // A single nameless parameter means a POST entity
                dynamic body = null;
                if (binder.CallInfo.ArgumentCount == 1 && binder.CallInfo.ArgumentNames.Count == 0)
                {
                    body = args[0];
                }
                var url = BuildUrl();
                var method = "GET";
                if (body != null)
                {
                    method = "POST";
                }
                var names = binder.CallInfo.ArgumentNames.Select(n => n.ToLowerInvariant()).ToList();
                method = GetMethodOverride(method, args, names);
                var queryString = BuildQueryString(names, args);
                result = _http.Execute(url, queryString, method, body);
                return true;
            }

            private static string BuildQueryString(IList<string> names, IList<object> values)
            {
                var sb = new StringBuilder();
                if (names.Any())
                {
                    for (var i = 0; i < values.Count; i++)
                    {
                        var name = names[i];
                        if (name.StartsWith(PrivateParameter))
                        {
                            continue;
                        }
                        var value = values[i];
                        if (value == null) continue;

                        sb.Append(i == 0 ? "?" : "&");
                        sb.Append(name).Append("=").Append(Uri.EscapeDataString(value.ToString()));
                    }
                }
                return sb.ToString();
            }

            private string BuildUrl()
            {
                var count = 0;
                var segments = new List<string>();
                if (_http._node != null)
                {
                    segments.Add(_http._node.Separator);
                    segments.Add(_http._node.Name);
                    count++;

                    var skipSeparator = false;
                    WalkSegments(segments, _http._node, ref count, ref skipSeparator);
                }
                else
                {
                    segments.Add(Separator);
                    segments.Add(Name);
                }

                var sb = new StringBuilder();
                sb.Append(_http.Endpoint);
                foreach (var segment in segments)
                {
                    sb.Append(segment);
                }
                var url = sb.ToString();
                return url;
            }

            private static void WalkSegments(ICollection<string> segments, UrlSegment node, ref int count, ref bool skipSeparator)
            {
                if (node._inner == null)
                {
                    return;
                }
                if (!skipSeparator)
                {
                    segments.Add(node._inner.Separator);
                }
                skipSeparator = node._inner.Separator.Equals(".");
                segments.Add(node._inner.Name);
                count++;
                WalkSegments(segments, node._inner, ref count, ref skipSeparator);
            }
        }
    }

    public class HttpReply
    {
        public HttpResponseMessage Response { get; set; }
        public HttpBody Body { get; set; }
    }

    public class HttpAuth
    {
        public static Action<Http> Basic(string token)
        {
            return Basic(token, "");
        }
        public static Action<Http> Basic(string username, string password)
        {
            return http =>
            {
                var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)));
                http.Headers.Add("Authorization", string.Format("Basic {0}", authorization));
            };
        }
        public static Action<Http> Ntlm()
        {
            return http =>
            {
                // This breaks contract, since it relies on an implementation
                HttpClientEngine.PerRequestHandler = HttpClientEngine.NtlmHandler;
            };
        }
    }
}

