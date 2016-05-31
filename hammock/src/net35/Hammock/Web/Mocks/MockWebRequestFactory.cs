using System;
using System.Net;
using System.Linq;
using Hammock.Extensions;

#if !SILVERLIGHT && !ClientProfiles && !MonoTouch
#if !NETCF
using System.Web;
#endif
using System.Collections.Specialized;
#endif

#if Silverlight
using Hammock.Silverlight.Compat;
#endif

#if ClientProfiles
using System.Collections.Specialized;
using System.Compat.Web;
#endif

#if SL3 || SL4
using System.Windows.Browser;
#endif

#if MonoTouch
using System.Collections.Specialized;
#endif

namespace Hammock.Web.Mocks
{
    internal class MockWebRequestFactory : IWebRequestCreate
    {
        public const string MockScheme = "mockScheme";
        public const string MockStatusCode = "mockStatusCode";
        public const string MockStatusDescription = "mockStatusDescription";
        public const string MockContent = "mockContent";
        public const string MockContentType = "mockContentType";
        public const string MockHeaderNames = "mockHeaderNames";
        public const string MockHeaderValues = "mockHeaderValues";
        public const string MockHttpMethod = "mockHttpMethod";

        public WebRequest Create(Uri uri)
        {
#if !SILVERLIGHT && !MonoTouch && !NETCF
            var query = HttpUtility.ParseQueryString(uri.Query);
#else
          var query = uri.Query.ParseQueryString();
#endif
            var scheme = query[MockScheme];
            var statusCode = query[MockStatusCode];
            var statusDescription = query[MockStatusDescription];
            var content = query[MockContent];
            var contentType = query[MockContentType];
            var headerNames = query[MockHeaderNames];
            var headerValues = query[MockHeaderValues];

            // Remove mocks parameters
            var queryString = new NameValueCollection();
#if !SILVERLIGHT && !MonoTouch && !NETCF
            foreach(var key in query.AllKeys)
#else
            foreach (var key in query.Keys)
#endif
            {
                if(key.EqualsAny(
                    MockScheme,
                    MockStatusCode,
                    MockStatusDescription,
                    MockContent,
                    MockContentType,
                    MockHeaderNames,
                    MockHeaderValues,
                    MockHttpMethod
                    ))
                {
                    continue;
                }
                queryString.Add(key, query[key]);
            }

            // [DC] Silverlight does not have uri.Authority
            var uriQuery = queryString.ToQueryString();
            var authority = "{0}{1}".FormatWith(
                uri.Host,
                (uri.Scheme.EqualsIgnoreCase("http") && uri.Port != 80 ||
                 uri.Scheme.EqualsIgnoreCase("https") && uri.Port != 443)
                    ? ":" + uri.Port
                    : "");

            var built = "{0}://{1}{2}{3}".FormatWithInvariantCulture(
                scheme, authority, uri.AbsolutePath, uriQuery
                );

            Uri mockUri;
            var request = Uri.TryCreate(
                built, UriKind.RelativeOrAbsolute, out mockUri
                ) ? new MockHttpWebRequest(mockUri)
                  : new MockHttpWebRequest(
                    new Uri(uri.ToString().Replace(
                              "mock", scheme))
                              );

            int statusCodeValue;
#if !NETCF
            int.TryParse(statusCode, out statusCodeValue);
#else
            try
            {
                statusCodeValue = int.Parse(statusCode);
            }
            catch (Exception)
            {
                statusCodeValue = 0;
            }
#endif
            if (!statusCode.IsNullOrBlank()) request.ExpectStatusCode = (HttpStatusCode)statusCodeValue;
            if (!statusDescription.IsNullOrBlank()) request.ExpectStatusDescription = statusDescription;
            if (!content.IsNullOrBlank()) request.Content = content;
            if (!contentType.IsNullOrBlank()) request.ContentType = contentType;

            if(!headerNames.IsNullOrBlank() && !headerValues.IsNullOrBlank())
            {
                var headers = new NameValueCollection();
                var names = headerNames.Split(',').Where(n => !n.IsNullOrBlank()).ToArray();
                var values = headerValues.Split(',').Where(v => !v.IsNullOrBlank()).ToArray();
                if(names.Count() == values.Count())
                {
                    for(var i = 0; i < names.Count(); i++)
                    {
                        headers.Add(names[i], values[i]);
                    }
                }

                foreach(var key in headers.AllKeys)
                {
                    request.ExpectHeaders.Add(key, headers[key]);
                }
            }
            return request;
        }
    }
}
