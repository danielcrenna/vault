using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cohort.Models;
using cohort.Mvc.Controllers;
using cohort.Sqlite;
using minirack;

namespace cohort.Mvc.Modules
{
    /// <summary>
    /// Logs all unhandled exceptions, logs 404 redirects, and routes errors to custom pages
    /// </summary>
    [Pipeline]
    public class ErrorModule : IHttpModule 
    {
        private readonly IBrokenLinkRepository _brokenLinks;

        public ErrorModule(IBrokenLinkRepository brokenLinks)
        {
            _brokenLinks = brokenLinks;
        }

        public ErrorModule() : this(new BrokenLinkRepository())
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.Error += UnhandledException;
        }

        private void UnhandledException(object sender, EventArgs eventArgs)
        {
            var error = new Error();
            var context = HttpContext.Current;
            Exception exception;
            var lastError = context.Server.GetLastError();
            for(exception = lastError; exception.InnerException != null; exception = exception.InnerException)
            {
                
            }

            var httpRequest = context.Request;
            if(exception is HttpException && ((HttpException)exception).GetHttpCode() == 404)
            {
                _brokenLinks.Save(new BrokenLink
                {
                    Path = httpRequest.Url.PathAndQuery,
                    Referer = httpRequest.UrlReferrer != null ? httpRequest.UrlReferrer.ToString() : null,
                    Method = httpRequest.HttpMethod
                });
            }

            RouteError(context, exception);
        }

        private static void RouteError(HttpContext context, Exception exception)
        {
            context.Response.Clear();

            var httpException = exception as HttpException;
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            if (httpException == null)
            {
                // Internal Server Error
                routeData.Values.Add("action", "Unknown");
            }
            else
            {
                var statusCode = httpException.GetHttpCode();
                context.Response.StatusCode = statusCode;
                switch (statusCode)
                {
                    case 404:
                        routeData.Values.Add("action", "NotFound");
                        break;
                    default:
                        routeData.Values.Add("action", "Unknown");
                        break;
                }
            }

            routeData.Values.Add("error", exception);
            context.Server.ClearError();

            IController errorController = new ErrorController();
            errorController.Execute(new RequestContext(new HttpContextWrapper(context), routeData));
        }

        public void Dispose()
        {
            
        }
    }
}