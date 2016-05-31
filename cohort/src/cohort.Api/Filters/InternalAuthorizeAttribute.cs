using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace cohort.API.Filters
{
    /// <summary>
    /// Unauthorized calls to internal API methods are hidden rather than returning a failed authorization.
    /// The failure message mimics legitimate 404 responses at the API endpoint
    /// </summary>
    public class InternalAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // http://blogs.msdn.com/b/youssefm/archive/2012/06/28/error-handling-in-asp-net-webapi.aspx
            var message = string.Format("No HTTP resource was found that matches the request URI '{0}'.", actionContext.Request.RequestUri);
            var error = new HttpError(message);
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
        }
    }
}