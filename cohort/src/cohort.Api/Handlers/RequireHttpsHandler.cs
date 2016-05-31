using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpContextShim;
using cohort.API.Models;

namespace cohort.API.Handlers
{
    /// <summary>
    /// Rejects API calls that are not over HTTPS
    /// </summary>
    public class RequireHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = HttpContext.Current;
            if (context == null || !context.Request.IsLocal)
            {
                var scheme = request.RequestUri.Scheme;
                if (!string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var response = request.CreateResponse(HttpStatusCode.Forbidden, Errors.RequiresHttps.ToHttpError());
                        response.Headers.Add("Connection", "close");
                        response.Headers.Add("Location", request.RequestUri.ToString().Replace("http://", "https://"));
                        return response;
                    }, cancellationToken);
                }
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}