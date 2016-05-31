// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Hammock.Silverlight.Compat;

namespace TweetSharp.Silverlight
{
    public sealed class HttpUtility
    {
        #region Fields

        private static Dictionary<string, char> entities;
        private static readonly object lock_ = new object();

        #endregion // Fields

        private static Dictionary<string, char> Entities
        {
            get
            {
                lock (lock_)
                {
                    if (entities == null)
                    {
                        InitEntities();
                    }

                    return entities;
                }
            }
        }

        #region Constructors

        private static void InitEntities()
        {
            // Build the hash table of HTML entity references.  This list comes
            // from the HTML 4.01 W3C recommendation.
            entities = new Dictionary<string, char>
                           {
                               {"nbsp", '\u00A0'},
                               {"iexcl", '\u00A1'},
                               {"cent", '\u00A2'},
                               {"pound", '\u00A3'},
                               {"curren", '\u00A4'},
                               {"yen", '\u00A5'},
                               {"brvbar", '\u00A6'},
                               {"sect", '\u00A7'},
                               {"uml", '\u00A8'},
                               {"copy", '\u00A9'},
                               {"ordf", '\u00AA'},
                               {"laquo", '\u00AB'},
                               {"not", '\u00AC'},
                               {"shy", '\u00AD'},
                               {"reg", '\u00AE'},
                               {"macr", '\u00AF'},
                               {"deg", '\u00B0'},
                               {"plusmn", '\u00B1'},
                               {"sup2", '\u00B2'},
                               {"sup3", '\u00B3'},
                               {"acute", '\u00B4'},
                               {"micro", '\u00B5'},
                               {"para", '\u00B6'},
                               {"middot", '\u00B7'},
                               {"cedil", '\u00B8'},
                               {"sup1", '\u00B9'},
                               {"ordm", '\u00BA'},
                               {"raquo", '\u00BB'},
                               {"frac14", '\u00BC'},
                               {"frac12", '\u00BD'},
                               {"frac34", '\u00BE'},
                               {"iquest", '\u00BF'},
                               {"Agrave", '\u00C0'},
                               {"Aacute", '\u00C1'},
                               {"Acirc", '\u00C2'},
                               {"Atilde", '\u00C3'},
                               {"Auml", '\u00C4'},
                               {"Aring", '\u00C5'},
                               {"AElig", '\u00C6'},
                               {"Ccedil", '\u00C7'},
                               {"Egrave", '\u00C8'},
                               {"Eacute", '\u00C9'},
                               {"Ecirc", '\u00CA'},
                               {"Euml", '\u00CB'},
                               {"Igrave", '\u00CC'},
                               {"Iacute", '\u00CD'},
                               {"Icirc", '\u00CE'},
                               {"Iuml", '\u00CF'},
                               {"ETH", '\u00D0'},
                               {"Ntilde", '\u00D1'},
                               {"Ograve", '\u00D2'},
                               {"Oacute", '\u00D3'},
                               {"Ocirc", '\u00D4'},
                               {"Otilde", '\u00D5'},
                               {"Ouml", '\u00D6'},
                               {"times", '\u00D7'},
                               {"Oslash", '\u00D8'},
                               {"Ugrave", '\u00D9'},
                               {"Uacute", '\u00DA'},
                               {"Ucirc", '\u00DB'},
                               {"Uuml", '\u00DC'},
                               {"Yacute", '\u00DD'},
                               {"THORN", '\u00DE'},
                               {"szlig", '\u00DF'},
                               {"agrave", '\u00E0'},
                               {"aacute", '\u00E1'},
                               {"acirc", '\u00E2'},
                               {"atilde", '\u00E3'},
                               {"auml", '\u00E4'},
                               {"aring", '\u00E5'},
                               {"aelig", '\u00E6'},
                               {"ccedil", '\u00E7'},
                               {"egrave", '\u00E8'},
                               {"eacute", '\u00E9'},
                               {"ecirc", '\u00EA'},
                               {"euml", '\u00EB'},
                               {"igrave", '\u00EC'},
                               {"iacute", '\u00ED'},
                               {"icirc", '\u00EE'},
                               {"iuml", '\u00EF'},
                               {"eth", '\u00F0'},
                               {"ntilde", '\u00F1'},
                               {"ograve", '\u00F2'},
                               {"oacute", '\u00F3'},
                               {"ocirc", '\u00F4'},
                               {"otilde", '\u00F5'},
                               {"ouml", '\u00F6'},
                               {"divide", '\u00F7'},
                               {"oslash", '\u00F8'},
                               {"ugrave", '\u00F9'},
                               {"uacute", '\u00FA'},
                               {"ucirc", '\u00FB'},
                               {"uuml", '\u00FC'},
                               {"yacute", '\u00FD'},
                               {"thorn", '\u00FE'},
                               {"yuml", '\u00FF'},
                               {"fnof", '\u0192'},
                               {"Alpha", '\u0391'},
                               {"Beta", '\u0392'},
                               {"Gamma", '\u0393'},
                               {"Delta", '\u0394'},
                               {"Epsilon", '\u0395'},
                               {"Zeta", '\u0396'},
                               {"Eta", '\u0397'},
                               {"Theta", '\u0398'},
                               {"Iota", '\u0399'},
                               {"Kappa", '\u039A'},
                               {"Lambda", '\u039B'},
                               {"Mu", '\u039C'},
                               {"Nu", '\u039D'},
                               {"Xi", '\u039E'},
                               {"Omicron", '\u039F'},
                               {"Pi", '\u03A0'},
                               {"Rho", '\u03A1'},
                               {"Sigma", '\u03A3'},
                               {"Tau", '\u03A4'},
                               {"Upsilon", '\u03A5'},
                               {"Phi", '\u03A6'},
                               {"Chi", '\u03A7'},
                               {"Psi", '\u03A8'},
                               {"Omega", '\u03A9'},
                               {"alpha", '\u03B1'},
                               {"beta", '\u03B2'},
                               {"gamma", '\u03B3'},
                               {"delta", '\u03B4'},
                               {"epsilon", '\u03B5'},
                               {"zeta", '\u03B6'},
                               {"eta", '\u03B7'},
                               {"theta", '\u03B8'},
                               {"iota", '\u03B9'},
                               {"kappa", '\u03BA'},
                               {"lambda", '\u03BB'},
                               {"mu", '\u03BC'},
                               {"nu", '\u03BD'},
                               {"xi", '\u03BE'},
                               {"omicron", '\u03BF'},
                               {"pi", '\u03C0'},
                               {"rho", '\u03C1'},
                               {"sigmaf", '\u03C2'},
                               {"sigma", '\u03C3'},
                               {"tau", '\u03C4'},
                               {"upsilon", '\u03C5'},
                               {"phi", '\u03C6'},
                               {"chi", '\u03C7'},
                               {"psi", '\u03C8'},
                               {"omega", '\u03C9'},
                               {"thetasym", '\u03D1'},
                               {"upsih", '\u03D2'},
                               {"piv", '\u03D6'},
                               {"bull", '\u2022'},
                               {"hellip", '\u2026'},
                               {"prime", '\u2032'},
                               {"Prime", '\u2033'},
                               {"oline", '\u203E'},
                               {"frasl", '\u2044'},
                               {"weierp", '\u2118'},
                               {"image", '\u2111'},
                               {"real", '\u211C'},
                               {"trade", '\u2122'},
                               {"alefsym", '\u2135'},
                               {"larr", '\u2190'},
                               {"uarr", '\u2191'},
                               {"rarr", '\u2192'},
                               {"darr", '\u2193'},
                               {"harr", '\u2194'},
                               {"crarr", '\u21B5'},
                               {"lArr", '\u21D0'},
                               {"uArr", '\u21D1'},
                               {"rArr", '\u21D2'},
                               {"dArr", '\u21D3'},
                               {"hArr", '\u21D4'},
                               {"forall", '\u2200'},
                               {"part", '\u2202'},
                               {"exist", '\u2203'},
                               {"empty", '\u2205'},
                               {"nabla", '\u2207'},
                               {"isin", '\u2208'},
                               {"notin", '\u2209'},
                               {"ni", '\u220B'},
                               {"prod", '\u220F'},
                               {"sum", '\u2211'},
                               {"minus", '\u2212'},
                               {"lowast", '\u2217'},
                               {"radic", '\u221A'},
                               {"prop", '\u221D'},
                               {"infin", '\u221E'},
                               {"ang", '\u2220'},
                               {"and", '\u2227'},
                               {"or", '\u2228'},
                               {"cap", '\u2229'},
                               {"cup", '\u222A'},
                               {"int", '\u222B'},
                               {"there4", '\u2234'},
                               {"sim", '\u223C'},
                               {"cong", '\u2245'},
                               {"asymp", '\u2248'},
                               {"ne", '\u2260'},
                               {"equiv", '\u2261'},
                               {"le", '\u2264'},
                               {"ge", '\u2265'},
                               {"sub", '\u2282'},
                               {"sup", '\u2283'},
                               {"nsub", '\u2284'},
                               {"sube", '\u2286'},
                               {"supe", '\u2287'},
                               {"oplus", '\u2295'},
                               {"otimes", '\u2297'},
                               {"perp", '\u22A5'},
                               {"sdot", '\u22C5'},
                               {"lceil", '\u2308'},
                               {"rceil", '\u2309'},
                               {"lfloor", '\u230A'},
                               {"rfloor", '\u230B'},
                               {"lang", '\u2329'},
                               {"rang", '\u232A'},
                               {"loz", '\u25CA'},
                               {"spades", '\u2660'},
                               {"clubs", '\u2663'},
                               {"hearts", '\u2665'},
                               {"diams", '\u2666'},
                               {"quot", '\u0022'},
                               {"amp", '\u0026'},
                               {"lt", '\u003C'},
                               {"gt", '\u003E'},
                               {"OElig", '\u0152'},
                               {"oelig", '\u0153'},
                               {"Scaron", '\u0160'},
                               {"scaron", '\u0161'},
                               {"Yuml", '\u0178'},
                               {"circ", '\u02C6'},
                               {"tilde", '\u02DC'},
                               {"ensp", '\u2002'},
                               {"emsp", '\u2003'},
                               {"thinsp", '\u2009'},
                               {"zwnj", '\u200C'},
                               {"zwj", '\u200D'},
                               {"lrm", '\u200E'},
                               {"rlm", '\u200F'},
                               {"ndash", '\u2013'},
                               {"mdash", '\u2014'},
                               {"lsquo", '\u2018'},
                               {"rsquo", '\u2019'},
                               {"sbquo", '\u201A'},
                               {"ldquo", '\u201C'},
                               {"rdquo", '\u201D'},
                               {"bdquo", '\u201E'},
                               {"dagger", '\u2020'},
                               {"Dagger", '\u2021'},
                               {"permil", '\u2030'},
                               {"lsaquo", '\u2039'},
                               {"rsaquo", '\u203A'},
                               {"euro", '\u20AC'}
                           };
        }

        #endregion // Constructors

        #region Methods

        private static readonly char[] hexChars = "0123456789abcdef".ToCharArray();

        public static void HtmlAttributeEncode(string s, TextWriter output)
        {
            output.Write(HtmlAttributeEncode(s));
        }

        public static string HtmlAttributeEncode(string s)
        {
            if (null == s)
            {
                return null;
            }

            var needEncode = false;
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == '&' || s[i] == '"' || s[i] == '<')
                {
                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
            {
                return s;
            }

            var output = new StringBuilder();
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                switch (s[i])
                {
                    case '&':
                        output.Append("&amp;");
                        break;
                    case '"':
                        output.Append("&quot;");
                        break;
                    case '<':
                        output.Append("&lt;");
                        break;
                    default:
                        output.Append(s[i]);
                        break;
                }
            }

            return output.ToString();
        }

        public static string UrlDecode(string str)
        {
            return UrlDecode(str, Encoding.UTF8);
        }

        private static char[] GetChars(MemoryStream b, Encoding e)
        {
            return e.GetChars(b.GetBuffer(), 0, (int) b.Length);
        }

        public static string UrlDecode(string s, Encoding e)
        {
            if (null == s)
            {
                return null;
            }

            if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
            {
                return s;
            }

            if (e == null)
            {
                e = Encoding.UTF8;
            }

            var output = new StringBuilder();
            long len = s.Length;
            var bytes = new MemoryStream();

            for (var i = 0; i < len; i++)
            {
                if (s[i] == '%' && i + 2 < len && s[i + 1] != '%')
                {
                    int xchar;
                    if (s[i + 1] == 'u' && i + 5 < len)
                    {
                        if (bytes.Length > 0)
                        {
                            output.Append(GetChars(bytes, e));
                            bytes.SetLength(0);
                        }

                        xchar = GetChar(s, i + 2, 4);
                        if (xchar != -1)
                        {
                            output.Append((char) xchar);
                            i += 5;
                        }
                        else
                        {
                            output.Append('%');
                        }
                    }
                    else if ((xchar = GetChar(s, i + 1, 2)) != -1)
                    {
                        bytes.WriteByte((byte) xchar);
                        i += 2;
                    }
                    else
                    {
                        output.Append('%');
                    }
                    continue;
                }

                if (bytes.Length > 0)
                {
                    output.Append(GetChars(bytes, e));
                    bytes.SetLength(0);
                }

                if (s[i] == '+')
                {
                    output.Append(' ');
                }
                else
                {
                    output.Append(s[i]);
                }
            }

            if (bytes.Length > 0)
            {
                output.Append(GetChars(bytes, e));
            }

            return output.ToString();
        }

        public static string UrlDecode(byte[] bytes, Encoding e)
        {
            return bytes == null ? null : UrlDecode(bytes, 0, bytes.Length, e);
        }

        private static int GetInt(byte b)
        {
            var c = (char) b;
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }

            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }

            return -1;
        }

        private static int GetChar(byte[] bytes, int offset, int length)
        {
            var value = 0;
            var end = length + offset;
            for (var i = offset; i < end; i++)
            {
                var current = GetInt(bytes[i]);
                if (current == -1)
                {
                    return -1;
                }
                value = (value << 4) + current;
            }

            return value;
        }

        private static int GetChar(string str, int offset, int length)
        {
            var val = 0;
            var end = length + offset;
            for (var i = offset; i < end; i++)
            {
                var c = str[i];
                if (c > 127)
                {
                    return -1;
                }

                var current = GetInt((byte) c);
                if (current == -1)
                {
                    return -1;
                }
                val = (val << 4) + current;
            }

            return val;
        }

        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            if (bytes == null)
            {
                return null;
            }
            if (count == 0)
            {
                return String.Empty;
            }

            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var output = new StringBuilder();
            var acc = new MemoryStream();

            var end = count + offset;
            for (var i = offset; i < end; i++)
            {
                if (bytes[i] == '%' && i + 2 < count && bytes[i + 1] != '%')
                {
                    int xchar;
                    if (bytes[i + 1] == (byte) 'u' && i + 5 < end)
                    {
                        if (acc.Length > 0)
                        {
                            output.Append(GetChars(acc, e));
                            acc.SetLength(0);
                        }
                        xchar = GetChar(bytes, i + 2, 4);
                        if (xchar != -1)
                        {
                            output.Append((char) xchar);
                            i += 5;
                            continue;
                        }
                    }
                    else if ((xchar = GetChar(bytes, i + 1, 2)) != -1)
                    {
                        acc.WriteByte((byte) xchar);
                        i += 2;
                        continue;
                    }
                }

                if (acc.Length > 0)
                {
                    output.Append(GetChars(acc, e));
                    acc.SetLength(0);
                }

                if (bytes[i] == '+')
                {
                    output.Append(' ');
                }
                else
                {
                    output.Append((char) bytes[i]);
                }
            }

            if (acc.Length > 0)
            {
                output.Append(GetChars(acc, e));
            }

            return output.ToString();
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes)
        {
            return bytes == null ? null : UrlDecodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlDecodeToBytes(string str)
        {
            return UrlDecodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }

            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            return UrlDecodeToBytes(e.GetBytes(str));
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            if (count == 0)
            {
                return new byte[0];
            }

            var len = bytes.Length;
            if (offset < 0 || offset >= len)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0 || offset > len - count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var result = new MemoryStream();
            var end = offset + count;
            for (var i = offset; i < end; i++)
            {
                var c = (char) bytes[i];
                if (c == '+')
                {
                    c = ' ';
                }
                else if (c == '%' && i < end - 2)
                {
                    var xchar = GetChar(bytes, i + 1, 2);
                    if (xchar != -1)
                    {
                        c = (char) xchar;
                        i += 2;
                    }
                }
                result.WriteByte((byte) c);
            }

            return result.ToArray();
        }

        public static string UrlEncode(string str)
        {
            return UrlEncode(str, Encoding.UTF8);
        }

        public static string UrlEncode(string s, Encoding enc)
        {
            if (s == null)
            {
                return null;
            }

            if (s == "")
            {
                return "";
            }

            var needEncode = false;
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                var c = s[i];
                if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z'))
                {
                    if (NotEncoded(c))
                    {
                        continue;
                    }

                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
            {
                return s;
            }

            // avoided GetByteCount call
            var bytes = new byte[enc.GetMaxByteCount(s.Length)];
            var realLen = enc.GetBytes(s, 0, s.Length, bytes, 0);
            return Encoding.UTF8.GetString(UrlEncodeToBytes(bytes, 0, realLen), 0, realLen);
        }

        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length == 0)
            {
                return "";
            }

            return Encoding.UTF8.GetString(UrlEncodeToBytes(bytes, 0, bytes.Length), 0, bytes.Length);
        }

        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length == 0)
            {
                return "";
            }

            return Encoding.UTF8.GetString(UrlEncodeToBytes(bytes, offset, count), offset, count);
        }

        public static byte[] UrlEncodeToBytes(string str)
        {
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }

            if (str == "")
            {
                return new byte[0];
            }

            var bytes = e.GetBytes(str);
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length == 0)
            {
                return new byte[0];
            }

            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        private static bool NotEncoded(char c)
        {
            return (c == '!' || c == '\'' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_');
        }

        private static void UrlEncodeChar(char c, Stream result, bool isUnicode)
        {
            if (c > 255)
            {
                //FIXME: what happens when there is an internal error?
                //if (!isUnicode)
                //	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
                int i = c;

                result.WriteByte((byte) '%');
                result.WriteByte((byte) 'u');
                var idx = i >> 12;
                result.WriteByte((byte) hexChars[idx]);
                idx = (i >> 8) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                idx = (i >> 4) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                idx = i & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                return;
            }

            if (c > ' ' && NotEncoded(c))
            {
                result.WriteByte((byte) c);
                return;
            }
            if (c == ' ')
            {
                result.WriteByte((byte) '+');
                return;
            }
            if ((c < '0') ||
                (c < 'A' && c > '9') ||
                (c > 'Z' && c < 'a') ||
                (c > 'z'))
            {
                if (isUnicode && c > 127)
                {
                    result.WriteByte((byte) '%');
                    result.WriteByte((byte) 'u');
                    result.WriteByte((byte) '0');
                    result.WriteByte((byte) '0');
                }
                else
                {
                    result.WriteByte((byte) '%');
                }

                var idx = (c) >> 4;
                result.WriteByte((byte) hexChars[idx]);
                idx = (c) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
            }
            else
            {
                result.WriteByte((byte) c);
            }
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }

            var len = bytes.Length;
            if (len == 0)
            {
                return new byte[0];
            }

            if (offset < 0 || offset >= len)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0 || count > len - offset)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var result = new MemoryStream(count);
            var end = offset + count;
            for (var i = offset; i < end; i++)
            {
                UrlEncodeChar((char) bytes[i], result, false);
            }

            return result.ToArray();
        }

        public static string UrlEncodeUnicode(string str)
        {
            if (str == null)
            {
                return null;
            }

            var bytes = UrlEncodeUnicodeToBytes(str);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeUnicodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str == "")
            {
                return new byte[0];
            }

            var result = new MemoryStream(str.Length);
            foreach (var c in str)
            {
                UrlEncodeChar(c, result, true);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Decodes an HTML-encoded string and returns the decoded string.
        /// </summary>
        /// <param name="s">The HTML string to decode. </param>
        /// <returns>The decoded text.</returns>
        public static string HtmlDecode(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (s.IndexOf('&') == -1)
            {
                return s;
            }

            var entity = new StringBuilder();
            var output = new StringBuilder();
            var len = s.Length;
            // 0 -> nothing,
            // 1 -> right after '&'
            // 2 -> between '&' and ';' but no '#'
            // 3 -> '#' found after '&' and getting numbers
            var state = 0;
            var number = 0;
            var have_trailing_digits = false;

            for (var i = 0; i < len; i++)
            {
                var c = s[i];
                if (state == 0)
                {
                    if (c == '&')
                    {
                        entity.Append(c);
                        state = 1;
                    }
                    else
                    {
                        output.Append(c);
                    }
                    continue;
                }

                if (c == '&')
                {
                    state = 1;
                    if (have_trailing_digits)
                    {
                        entity.Append(number.ToString(CultureInfo.InvariantCulture));
                        have_trailing_digits = false;
                    }

                    output.Append(entity.ToString());
                    entity.Length = 0;
                    entity.Append('&');
                    continue;
                }

                if (state == 1)
                {
                    if (c == ';')
                    {
                        state = 0;
                        output.Append(entity);
                        output.Append(c);
                        entity.Length = 0;
                    }
                    else
                    {
                        number = 0;
                        state = c != '#' ? 2 : 3;
                        entity.Append(c);
                    }
                }
                else if (state == 2)
                {
                    entity.Append(c);
                    if (c == ';')
                    {
                        var key = entity.ToString();
                        if (key.Length > 1 && Entities.ContainsKey(key.Substring(1, key.Length - 2)))
                        {
                            key = Entities[key.Substring(1, key.Length - 2)].ToString();
                        }

                        output.Append(key);
                        state = 0;
                        entity.Length = 0;
                    }
                }
                else if (state == 3)
                {
                    if (c == ';')
                    {
                        if (number > 65535)
                        {
                            output.Append("&#");
                            output.Append(number.ToString(CultureInfo.InvariantCulture));
                            output.Append(";");
                        }
                        else
                        {
                            output.Append((char) number);
                        }
                        state = 0;
                        entity.Length = 0;
                        have_trailing_digits = false;
                    }
                    else if (Char.IsDigit(c))
                    {
                        number = number*10 + (c - '0');
                        have_trailing_digits = true;
                    }
                    else
                    {
                        state = 2;
                        if (have_trailing_digits)
                        {
                            entity.Append(number.ToString(CultureInfo.InvariantCulture));
                            have_trailing_digits = false;
                        }
                        entity.Append(c);
                    }
                }
            }

            if (entity.Length > 0)
            {
                output.Append(entity.ToString());
            }
            else if (have_trailing_digits)
            {
                output.Append(number.ToString(CultureInfo.InvariantCulture));
            }
            return output.ToString();
        }

        /// <summary>
        /// Decodes an HTML-encoded string and sends the resulting output to a TextWriter output stream.
        /// </summary>
        /// <param name="s">The HTML string to decode</param>
        /// <param name="output">The TextWriter output stream containing the decoded string. </param>
        public static void HtmlDecode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(HtmlDecode(s));
            }
        }

        /// <summary>
        /// HTML-encodes a string and returns the encoded string.
        /// </summary>
        /// <param name="s">The text string to encode. </param>
        /// <returns>The HTML-encoded text.</returns>
        public static string HtmlEncode(string s)
        {
            if (s == null)
            {
                return null;
            }

            var needEncode = s.Any(c => c == '&' || c == '"' || c == '<' || c == '>' || c > 159);

            if (!needEncode)
            {
                return s;
            }

            var output = new StringBuilder();

            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                switch (s[i])
                {
                    case '&':
                        output.Append("&amp;");
                        break;
                    case '>':
                        output.Append("&gt;");
                        break;
                    case '<':
                        output.Append("&lt;");
                        break;
                    case '"':
                        output.Append("&quot;");
                        break;
                    default:
                        // MS starts encoding with &# from 160 and stops at 255.
                        // We don't do that. One reason is the 65308/65310 unicode
                        // characters that look like '<' and '>'.
#if TARGET_JVM
					if (s [i] > 159 && s [i] < 256) {
#else
                        if (s[i] > 159)
                        {
#endif
                            output.Append("&#");
                            output.Append(((int) s[i]).ToString(CultureInfo.InvariantCulture));
                            output.Append(";");
                        }
                        else
                        {
                            output.Append(s[i]);
                        }
                        break;
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// HTML-encodes a string and sends the resulting output to a TextWriter output stream.
        /// </summary>
        /// <param name="s">The string to encode. </param>
        /// <param name="output">The TextWriter output stream containing the encoded string. </param>
        public static void HtmlEncode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(HtmlEncode(s));
            }
        }

        public static string UrlPathEncode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            var result = new MemoryStream();
            var length = s.Length;
            for (var i = 0; i < length; i++)
            {
                UrlPathEncodeChar(s[i], result);
            }

            var bytes = result.ToArray();
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        private static void UrlPathEncodeChar(char c, Stream result)
        {
            if (c < 33 || c > 126)
            {
                var bIn = Encoding.UTF8.GetBytes(c.ToString());
                for (var i = 0; i < bIn.Length; i++)
                {
                    result.WriteByte((byte) '%');
                    var idx = bIn[i] >> 4;
                    result.WriteByte((byte) hexChars[idx]);
                    idx = bIn[i] & 0x0F;
                    result.WriteByte((byte) hexChars[idx]);
                }
            }
            else if (c == ' ')
            {
                result.WriteByte((byte) '%');
                result.WriteByte((byte) '2');
                result.WriteByte((byte) '0');
            }
            else
                result.WriteByte((byte) c);
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
                return new NameValueCollection();
            if (query[0] == '?')
                query = query.Substring(1);

            var result = new NameValueCollection();
            ParseQueryString(query, encoding, result);
            return result;
        }

        internal static void ParseQueryString(string query, Encoding encoding, NameValueCollection result)
        {
            if (query.Length == 0)
            {
                return;
            }

            var decoded = HtmlDecode(query);
            var decodedLength = decoded.Length;
            var namePos = 0;
            var first = true;
            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (var q = namePos; q < decodedLength; q++)
                {
                    if (valuePos == -1 && decoded[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (decoded[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (decoded[namePos] == '?')
                    {
                        namePos++;
                    }
                }

                string name;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1), encoding);
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = decoded.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                var value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos), encoding);

                result.Add(name, value);
                if (namePos == -1)
                {
                    break;
                }
            }
        }

        #endregion // Methods
    }
}