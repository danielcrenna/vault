using System.Linq;
using System.Web.Mvc;

namespace cohort.Mvc.Filters
{
    /// <summary>
    /// By default, renders all returned cookies from the server as HttpOnly and secure (if not local)
    /// <seealso href="http://stackoverflow.com/questions/6377134/secure-forms-authentication-behind-proxy" />
    /// </summary>
    public class SecureCookiesFilterAttribute : FilterAttribute, IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var cookieNames = filterContext.HttpContext.Response.Cookies.AllKeys;

            var httpCookies = cookieNames
                .Select(cookieName => filterContext.HttpContext.Response.Cookies[cookieName])
                .Where(httpCookie => httpCookie != null)
                .ToList();

            foreach (var httpCookie in httpCookies)
            {
                httpCookie.HttpOnly = true;
            }

            if (filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            foreach (var httpCookie in httpCookies)
            {
                httpCookie.Secure = true;
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}