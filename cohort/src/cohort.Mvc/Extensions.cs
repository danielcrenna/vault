using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace cohort.Mvc
{
    internal static class Extensions
    {
        internal static string GetLinkForRoute(this RequestContext context, RouteValueDictionary data)
        {
            string url = null;
            var request = context.HttpContext.Request;
            if (request != null && request.Url != null)
            {
                var helper = new UrlHelper(context, RouteTable.Routes);
                var port = request.Url.Port != 80 ? ":" + request.Url.Port : "";
                var server = request.ServerVariables["SERVER_NAME"];
                url = String.Concat("http://", server, port, helper.RouteUrl(data));
            }
            return url;
        }









        // http://www.zvolkov.com/clog/2012/04/01/asp-net-mvc-build-url-based-on-current-url/
        internal static MvcHtmlString Current(this UrlHelper helper, object substitutes)
        {
            return GetCurrentRoute(helper, substitutes);
        }
        internal static string Current(this ControllerContext context, object substitutes)
        {
            return GetCurrentRoute(new UrlHelper(context.RequestContext), RouteTable.Routes).ToString();
        }
        internal static MvcHtmlString GetCurrentRoute(UrlHelper helper, object substitutes)
        {
            var rd = new RouteValueDictionary(helper.RequestContext.RouteData.Values);
            var qs = helper.RequestContext.HttpContext.Request.QueryString;
            foreach (var param in qs.Cast<string>().Where(param => !String.IsNullOrEmpty(qs[param])))
            {
                rd[param] = qs[param];
            }
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(substitutes.GetType()))
            {
                var value = property.GetValue(substitutes).ToString();
                if (String.IsNullOrEmpty(value)) rd.Remove(property.Name);
                else rd[property.Name] = value;

                var url = helper.RouteUrl(rd);
                return new MvcHtmlString(url);
            }
            return null;
        }

        
    }
}
