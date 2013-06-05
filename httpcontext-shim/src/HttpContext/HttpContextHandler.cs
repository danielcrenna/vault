using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpContextShim.SelfHost;
using HttpContextShim.WebHost;

namespace HttpContextShim
{
    /// <summary>
    /// Provides support for the general availability of an HttpContext in either WebHost or SelfHost scenarios.
    /// <remarks>
    /// This should always be the first handler, or at least before any handlers that require HttpContext access
    /// </remarks>
    /// </summary>
    public class HttpContextHandler : DelegatingHandler
    {
        private const string HttpContextProperty = "MS_HttpContext";
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            object contextPropertyValue;
            request.Properties.TryGetValue(HttpContextProperty, out contextPropertyValue);
            var context = contextPropertyValue as HttpContextBase;
            
            HttpContext.Current = context != null ? (IHttpContext)new AspNetHttpContext(context) : new SelfHostHttpContext(request); 
            return base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {
                var result = task.Result;
                if(context == null)
                {
                    ((SelfHostHttpContext)HttpContext.Current).SetResponse(result);
                }
                return result;
            });
        }
    }
}

