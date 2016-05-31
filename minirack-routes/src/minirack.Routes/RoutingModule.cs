using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace minirack.Routes
{
    [Pipeline]
    public class PipelineModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, args) =>
            {
                var request = context.Request;
                var response = context.Response;
                CanonicalRoutes(request, response);
            };
        }

        private static void CanonicalRoutes(HttpRequest request, HttpResponse response)
        {
            if (!Routing.CanonicalLowercase && !Routing.CanonicalRemoveTrailingSlash)
            {
                return;
            }
            var absolutePath = request.Url.AbsolutePath;
            if (Routing.CanonicalRemoveTrailingSlash && absolutePath.Equals("/"))
            {
                absolutePath = string.Empty;
            }
            var original = string.Concat(request.Url.Scheme, "://", request.Url.Authority, absolutePath);
            var candidate = CanonicalLowercase(original);
            candidate = CanonicalRemoveTrailingSlash(candidate);
            original = string.Concat(original, request.Url.Query);
            candidate = string.Concat(candidate, request.Url.Query);
            if (!original.Equals(candidate))
            {
                PermanentRedirect(response, candidate);
            }
        }

        private static string CanonicalLowercase(string baseUrl)
        {
            if (!Routing.CanonicalLowercase)
            {
                return baseUrl;
            }
            return !Regex.IsMatch(baseUrl, @"[A-Z]", RegexOptions.Compiled) ? baseUrl : baseUrl.ToLowerInvariant();
        }

        private static string CanonicalRemoveTrailingSlash(string baseUrl)
        {
            if (!Routing.CanonicalRemoveTrailingSlash)
            {
                return baseUrl;
            }
            return !baseUrl.EndsWith("/") ? baseUrl : baseUrl.TrimEnd('/');
        }

        private static void PermanentRedirect(HttpResponse response, string location)
        {
            response.Clear();
            response.StatusCode = (int)HttpStatusCode.MovedPermanently;
            response.StatusDescription = "Moved Permanently";
            response.AddHeader("Location", location);
            response.End();
        }

        public void Dispose()
        {

        }
    }
}