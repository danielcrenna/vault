using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace cohort.API.Filters
{
    public class ErrorFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is NotImplementedException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }
        }
    }
}