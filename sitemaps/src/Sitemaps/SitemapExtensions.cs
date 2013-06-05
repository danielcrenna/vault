using System;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Sitemaps
{
    internal static class SitemapExtensions
    {
        public static bool MatchesRouteWithHttpGet(this string url)
        {
            var path = VirtualPathUtility.ToAppRelative(new Uri(url).AbsolutePath);
            var fakeContext = new FakeHttpContext(path, "GET");
            using(RouteTable.Routes.GetReadLock())
            {
                return RouteTable.Routes.Aggregate(false, (current, route) =>
                {
                    if (!fakeContext.Request.AppRelativeCurrentExecutionFilePath.Equals("~/"))
                    {
                        var routeData = route.GetRouteData(fakeContext);
                        var matches = routeData != null;
                        return current | matches;
                    }
                    return true;
                });
            }
        }

        private class FakeHttpContext : HttpContextBase
        {
            private readonly string _relativeUrl;
            private readonly HttpRequestBase _httpRequest;
            private readonly string _httpMethod;

            public FakeHttpContext(string relativeUrl, string httpMethod)
            {
                _relativeUrl = relativeUrl;
                _httpMethod = httpMethod;
                _httpRequest = new FakeHttpRequest(_relativeUrl, _httpMethod);
            }

            public override HttpRequestBase Request
            {
                get
                {
                    return _httpRequest;
                }
            }
        }

        private class FakeHttpRequest : HttpRequestBase
        {
            private readonly string _relativeUrl;
            private readonly string _httpMethod;

            public FakeHttpRequest(string relativeUrl, string httpMethod)
            {
                _relativeUrl = relativeUrl;
                _httpMethod = httpMethod;
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return _relativeUrl; }
            }

            public override string PathInfo
            {
                get { return ""; }
            }

            public override string HttpMethod
            {
                get
                {
                    return _httpMethod;
                }
            }
        }
    }
}
