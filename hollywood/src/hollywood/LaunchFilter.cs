using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace hollywood
{
    /// <summary>
    /// Prevents access to the site, redirecting all requests to a prelaunch page,
    /// unless the user passes in a secret token set up in web.config into the
    /// query string.
    /// </summary>
    public class LaunchFilter : ActionFilterAttribute
    {
        private static string _tokenValue;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;
            if(context == null || context.Session == null)
            {
                return;
            }

            string url;
            if (RouteShouldBeBypassed(filterContext, context, out url))
            {
                return;
            }

            filterContext.Result = new RedirectResult(url);
        }

        private static bool RouteShouldBeBypassed(ActionExecutingContext filterContext, HttpContextBase context, out string url)
        {
            var helper = new UrlHelper(filterContext.RequestContext);
            url = helper.Action("Index", "Launch", new {area = ""});
            if (context.Request.Path.Equals(url))
            {
                return true;
            }

            // Add allowed routes here
            return Launch.AllowedUrls.Value.Any(allowed => context.Request.Path.Equals(allowed)) || UserIsAllowedToSeeSite(filterContext, context);
        }

        private static bool UserIsAllowedToSeeSite(ActionExecutingContext filterContext, HttpContextBase context)
        {
            var isPrelaunch = ConfigurationManager.AppSettings["Prelaunch"];
            var isLaunched = !Convert.ToBoolean(isPrelaunch);
            if (isLaunched)
            {
                return true;
            }
            var request = context.Request;
            _tokenValue = _tokenValue ?? ConfigurationManager.AppSettings["PrelaunchToken"];
            if (context.Session != null && (context.Session["token"] != null && context.Session["token"].Equals(_tokenValue)))
            {
                return true;
            }
            if (request != null)
            {
                var token = request.QueryString["token"];
                if (token != null && token.Equals(_tokenValue))
                {
                    if (context.Session != null) context.Session["token"] = _tokenValue;
                    filterContext.Result = new RedirectResult("/");
                    return true;
                }
            }
            return false;
        }
    }
}