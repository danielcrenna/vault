//
// System.Net.WebHeaderCollection
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright 2007 Novell, Inc. (http://www.novell.com)
//
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

// See RFC 2068 par 4.2 Message Headers

namespace Mono.Net
{
    [Serializable]
    [ComVisible(true)]
    public class WebHeaderCollection : NameValueCollection
    {
        private static readonly Hashtable _restricted;
        private static readonly Hashtable _multiValue;

        private static readonly char[] tspecials =
            new[]
                {
                    '(', ')', '<', '>', '@',
                    ',', ';', ':', '\\', '"',
                    '/', '[', ']', '?', '=',
                    '{', '}', ' ', '\t'
                };

        private readonly bool internallyCreated;

        // Static Initializer

        static WebHeaderCollection()
        {
            // the list of restricted header names as defined 
            // by the ms.net spec
            _restricted = new Hashtable(StringComparer.InvariantCultureIgnoreCase)
                             {
                                 {"accept", true},
                                 {"connection", true},
                                 {"content-length", true},
                                 {"content-type", true},
                                 {"date", true},
                                 {"expect", true},
                                 {"host", true},
                                 {"if-modified-since", true},
                                 {"range", true},
                                 {"referer", true},
                                 {"transfer-encoding", true},
                                 {"user-agent", true}
                             };

            // see par 14 of RFC 2068 to see which header names
            // accept multiple values each separated by a comma
            _multiValue = new Hashtable(StringComparer.InvariantCultureIgnoreCase)
                             {
                                 {"accept", true},
                                 {"accept-charset", true},
                                 {"accept-encoding", true},
                                 {"accept-language", true},
                                 {"accept-ranges", true},
                                 {"allow", true},
                                 {"authorization", true},
                                 {"cache-control", true},
                                 {"connection", true},
                                 {"content-encoding", true},
                                 {"content-language", true},
                                 {"expect", true},
                                 {"if-match", true},
                                 {"if-none-match", true},
                                 {"proxy-authenticate", true},
                                 {"public", true},
                                 {"range", true},
                                 {"transfer-encoding", true},
                                 {"upgrade", true},
                                 {"vary", true},
                                 {"via", true},
                                 {"warning", true},
                                 {"www-authenticate", true},
                                 {"set-cookie", true},
                                 {"set-cookie2", true}
                             };

            // Extra
        }

        // Constructors

        public WebHeaderCollection()
        {
        }

        internal WebHeaderCollection(bool internallyCreated)
        {
            this.internallyCreated = internallyCreated;
        }

        public override string[] AllKeys
        {
            get { return (base.AllKeys); }
        }

        public override int Count
        {
            get { return (base.Count); }
        }

        public override KeysCollection Keys
        {
            get { return (base.Keys); }
        }

        public string this[HttpRequestHeader hrh]
        {
            get { return Get(RequestHeaderToString(hrh)); }

            set { Add(RequestHeaderToString(hrh), value); }
        }

        public string this[HttpResponseHeader hrh]
        {
            get { return Get(ResponseHeaderToString(hrh)); }

            set { Add(ResponseHeaderToString(hrh), value); }
        }

        // Methods

        public void Add(string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            var pos = header.IndexOf(':');
            if (pos == -1)
                throw new ArgumentException("no colon found", "header");
            Add(header.Substring(0, pos),
                header.Substring(pos + 1));
        }

        public override void Add(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (internallyCreated && IsRestricted(name))
                throw new ArgumentException("This header must be modified with the appropiate property.");
            AddWithoutValidate(name, value);
        }

        protected void AddWithoutValidate(string headerName, string headerValue)
        {
            if (!IsHeaderName(headerName))
                throw new ArgumentException("invalid header name: " + headerName, "headerName");
            headerValue = headerValue == null ? String.Empty : headerValue.Trim();
            if (!IsHeaderValue(headerValue))
                throw new ArgumentException("invalid header value: " + headerValue, "headerValue");
            base.Add(headerName, headerValue);
        }

        public override string[] GetValues(string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            var values = base.GetValues(header);
            if (values == null || values.Length == 0)
                return null;

            /*
            if (IsMultiValue (header)) {
                values = GetMultipleValues (values);
            }
            */

            return values;
        }

        public override string[] GetValues(int index)
        {
            var values = base.GetValues(index);
            if (values == null || values.Length == 0)
            {
                return (null);
            }

            return (values);
        }

        /* Now i wonder why this is here...
        static string [] GetMultipleValues (string [] values)
        {
            ArrayList mvalues = new ArrayList (values.Length);
            StringBuilder sb = null;
            for (int i = 0; i < values.Length; ++i) {
                string val = values [i];
                if (val.IndexOf (',') == -1) {
                    mvalues.Add (val);
                    continue;
                }

                if (sb == null)
                    sb = new StringBuilder ();

                bool quote = false;
                for (int k = 0; k < val.Length; k++) {
                    char c = val [k];
                    if (c == '"') {
                        quote = !quote;
                    } else if (!quote && c == ',') {
                        mvalues.Add (sb.ToString ().Trim ());
                        sb.Length = 0;
                        continue;
                    }
                    sb.Append (c);
                }

                if (sb.Length > 0) {
                    mvalues.Add (sb.ToString ().Trim ());
                    sb.Length = 0;
                }
            }

            return (string []) mvalues.ToArray (typeof (string));
        }
        */

        public static bool IsRestricted(string headerName)
        {
            if (headerName == null)
                throw new ArgumentNullException("headerName");

            if (headerName == "") // MS throw nullexception here!
                throw new ArgumentException("empty string", "headerName");

            return _restricted.ContainsKey(headerName);
        }

        public static bool IsRestricted(string headerName, bool response)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (internallyCreated && IsRestricted(name))
                throw new ArgumentException("restricted header");
            base.Remove(name);
        }

        public override void Set(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (internallyCreated && IsRestricted(name))
                throw new ArgumentException("restricted header");
            if (!IsHeaderName(name))
                throw new ArgumentException("invalid header name");
            value = value == null ? String.Empty : value.Trim();
            if (!IsHeaderValue(value))
                throw new ArgumentException("invalid header value");
            base.Set(name, value);
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var count = base.Count;
            for (var i = 0; i < count; i++)
                sb.Append(GetKey(i))
                    .Append(": ")
                    .Append(Get(i))
                    .Append("\r\n");

            return sb.Append("\r\n").ToString();
        }

        public override string Get(int index)
        {
            return (base.Get(index));
        }

        public override string Get(string name)
        {
            return (base.Get(name));
        }

        public override string GetKey(int index)
        {
            return (base.GetKey(index));
        }

        public void Add(HttpRequestHeader header, string value)
        {
            Add(RequestHeaderToString(header), value);
        }

        public void Remove(HttpRequestHeader header)
        {
            Remove(RequestHeaderToString(header));
        }

        public void Set(HttpRequestHeader header, string value)
        {
            Set(RequestHeaderToString(header), value);
        }

        public void Add(HttpResponseHeader header, string value)
        {
            Add(ResponseHeaderToString(header), value);
        }

        public void Remove(HttpResponseHeader header)
        {
            Remove(ResponseHeaderToString(header));
        }

        public void Set(HttpResponseHeader header, string value)
        {
            Set(ResponseHeaderToString(header), value);
        }

        private static string RequestHeaderToString(HttpRequestHeader value)
        {
            switch (value)
            {
                case HttpRequestHeader.CacheControl:
                    return "cache-control";
                case HttpRequestHeader.Connection:
                    return "connection";
                case HttpRequestHeader.Date:
                    return "date";
                case HttpRequestHeader.KeepAlive:
                    return "keep-alive";
                case HttpRequestHeader.Pragma:
                    return "pragma";
                case HttpRequestHeader.Trailer:
                    return "trailer";
                case HttpRequestHeader.TransferEncoding:
                    return "transfer-encoding";
                case HttpRequestHeader.Upgrade:
                    return "upgrade";
                case HttpRequestHeader.Via:
                    return "via";
                case HttpRequestHeader.Warning:
                    return "warning";
                case HttpRequestHeader.Allow:
                    return "allow";
                case HttpRequestHeader.ContentLength:
                    return "content-length";
                case HttpRequestHeader.ContentType:
                    return "content-type";
                case HttpRequestHeader.ContentEncoding:
                    return "content-encoding";
                case HttpRequestHeader.ContentLanguage:
                    return "content-language";
                case HttpRequestHeader.ContentLocation:
                    return "content-location";
                case HttpRequestHeader.ContentMd5:
                    return "content-md5";
                case HttpRequestHeader.ContentRange:
                    return "content-range";
                case HttpRequestHeader.Expires:
                    return "expires";
                case HttpRequestHeader.LastModified:
                    return "last-modified";
                case HttpRequestHeader.Accept:
                    return "accept";
                case HttpRequestHeader.AcceptCharset:
                    return "accept-charset";
                case HttpRequestHeader.AcceptEncoding:
                    return "accept-encoding";
                case HttpRequestHeader.AcceptLanguage:
                    return "accept-language";
                case HttpRequestHeader.Authorization:
                    return "authorization";
                case HttpRequestHeader.Cookie:
                    return "cookie";
                case HttpRequestHeader.Expect:
                    return "expect";
                case HttpRequestHeader.From:
                    return "from";
                case HttpRequestHeader.Host:
                    return "host";
                case HttpRequestHeader.IfMatch:
                    return "if-match";
                case HttpRequestHeader.IfModifiedSince:
                    return "if-modified-since";
                case HttpRequestHeader.IfNoneMatch:
                    return "if-none-match";
                case HttpRequestHeader.IfRange:
                    return "if-range";
                case HttpRequestHeader.IfUnmodifiedSince:
                    return "if-unmodified-since";
                case HttpRequestHeader.MaxForwards:
                    return "max-forwards";
                case HttpRequestHeader.ProxyAuthorization:
                    return "proxy-authorization";
                case HttpRequestHeader.Referer:
                    return "referer";
                case HttpRequestHeader.Range:
                    return "range";
                case HttpRequestHeader.Te:
                    return "te";
                case HttpRequestHeader.Translate:
                    return "translate";
                case HttpRequestHeader.UserAgent:
                    return "user-agent";
                default:
                    throw new InvalidOperationException();
            }
        }


        private static string ResponseHeaderToString(HttpResponseHeader value)
        {
            switch (value)
            {
                case HttpResponseHeader.CacheControl:
                    return "cache-control";
                case HttpResponseHeader.Connection:
                    return "connection";
                case HttpResponseHeader.Date:
                    return "date";
                case HttpResponseHeader.KeepAlive:
                    return "keep-alive";
                case HttpResponseHeader.Pragma:
                    return "pragma";
                case HttpResponseHeader.Trailer:
                    return "trailer";
                case HttpResponseHeader.TransferEncoding:
                    return "transfer-encoding";
                case HttpResponseHeader.Upgrade:
                    return "upgrade";
                case HttpResponseHeader.Via:
                    return "via";
                case HttpResponseHeader.Warning:
                    return "warning";
                case HttpResponseHeader.Allow:
                    return "allow";
                case HttpResponseHeader.ContentLength:
                    return "content-length";
                case HttpResponseHeader.ContentType:
                    return "content-type";
                case HttpResponseHeader.ContentEncoding:
                    return "content-encoding";
                case HttpResponseHeader.ContentLanguage:
                    return "content-language";
                case HttpResponseHeader.ContentLocation:
                    return "content-location";
                case HttpResponseHeader.ContentMd5:
                    return "content-md5";
                case HttpResponseHeader.ContentRange:
                    return "content-range";
                case HttpResponseHeader.Expires:
                    return "expires";
                case HttpResponseHeader.LastModified:
                    return "last-modified";
                case HttpResponseHeader.AcceptRanges:
                    return "accept-ranges";
                case HttpResponseHeader.Age:
                    return "age";
                case HttpResponseHeader.ETag:
                    return "etag";
                case HttpResponseHeader.Location:
                    return "location";
                case HttpResponseHeader.ProxyAuthenticate:
                    return "proxy-authenticate";
                case HttpResponseHeader.RetryAfter:
                    return "RetryAfter";
                case HttpResponseHeader.Server:
                    return "server";
                case HttpResponseHeader.SetCookie:
                    return "set-cookie";
                case HttpResponseHeader.Vary:
                    return "vary";
                case HttpResponseHeader.WwwAuthenticate:
                    return "www-authenticate";
                default:
                    throw new InvalidOperationException();
            }
        }

        public override IEnumerator GetEnumerator()
        {
            return (base.GetEnumerator());
        }

        // Internal Methods

        // With this we don't check for invalid characters in header. See bug #55994.
        internal void SetInternal(string header)
        {
            var pos = header.IndexOf(':');
            if (pos == -1)
                throw new ArgumentException("no colon found", "header");

            SetInternal(header.Substring(0, pos), header.Substring(pos + 1));
        }

        internal void SetInternal(string name, string value)
        {
            value = value == null ? String.Empty : value.Trim();
            if (!IsHeaderValue(value))
                throw new ArgumentException("invalid header value");

            if (IsMultiValue(name))
            {
                base.Add(name, value);
            }
            else
            {
                base.Remove(name);
                base.Set(name, value);
            }
        }

        internal void RemoveAndAdd(string name, string value)
        {
            value = value == null ? String.Empty : value.Trim();

            base.Remove(name);
            base.Set(name, value);
        }

        internal void RemoveInternal(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            base.Remove(name);
        }

        internal static bool IsMultiValue(string headerName)
        {
            return !string.IsNullOrEmpty(headerName) && _multiValue.ContainsKey(headerName);
        }

        internal static bool IsHeaderValue(string value)
        {
            // TEXT any 8 bit value except CTL's (0-31 and 127)
            //      but including \r\n space and \t
            //      after a newline at least one space or \t must follow
            //      certain header fields allow comments ()

            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                var c = value[i];
                if (c == 127)
                    return false;
                if (c < 0x20 && (c != '\r' && c != '\n' && c != '\t'))
                    return false;
                if (c == '\n' && ++i < len)
                {
                    c = value[i];
                    if (c != ' ' && c != '\t')
                        return false;
                }
            }

            return true;
        }

        internal static bool IsHeaderName(string name)
        {
            // token          = 1*<any CHAR except CTLs or tspecials>
            // tspecials      = "(" | ")" | "<" | ">" | "@"
            //                | "," | ";" | ":" | "\" | <">
            //                | "/" | "[" | "]" | "?" | "="
            //                | "{" | "}" | SP | HT

            if (string.IsNullOrEmpty(name))
                return false;

            var len = name.Length;
            for (var i = 0; i < len; i++)
            {
                var c = name[i];
                if (c < 0x20 || c >= 0x7f)
                    return false;
            }

            return name.IndexOfAny(tspecials) == -1;
        }
    }
}