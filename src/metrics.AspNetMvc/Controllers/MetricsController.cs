using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using metrics.AspNetMvc.Extensions;
using metrics.Core;
using metrics.Serialization;

namespace metrics.AspNetMvc.Controllers
{
    /// <summary>
    /// A controller for metrics API methods
    /// </summary>
    public class MetricsController : Controller
    {
        /// <summary>
        /// A JSON object of all registered metrics and a host of CLR metrics
        /// </summary>
        public ActionResult Metrics(string username, string password)
        {
            if (!IsAuthorized(username, password))
            {
                return new HttpStatusCodeResult(401);
            }

            var result = new ContentResult
                             {
                                 Content = Serializer.Serialize(metrics.Metrics.All),
                                 ContentType = "application/json",
                                 ContentEncoding = Encoding.UTF8
                             };

            return result;
        }
        
        /// <summary>
        /// A simple text/plain "pong" for load-balancers
        /// </summary>
        public ActionResult Ping(string username, string password)
        {
            if (!IsAuthorized(username, password))
            {
                return new HttpStatusCodeResult(401);
            }

            var result = new ContentResult
                             {
                                 Content = "pong",
                                 ContentType = "text/plain",
                                 ContentEncoding = Encoding.UTF8
                             };

            return result;
        }

        /// <summary>
        /// Runs through all registered HealthCheck instances and reports the results. 
        /// Returns a `200 OK` if all succeeded, or a `500 Internal Server Error` if any failed.
        /// </summary>
        public ActionResult HealthCheck(string username, string password)
        {
            if (!IsAuthorized(username, password))
            {
                return new HttpStatusCodeResult(401);
            }

            var health = HealthChecks.RunHealthChecks();
            ControllerContext.HttpContext.Response.StatusCode = health.Values.Where(v => v.IsHealthy).Count() != health.Values.Count ? 500 : 200;

            var result = new ContentResult
            {
                Content = Serializer.Serialize(health),
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8
            };

            return result;
        }

        /// <summary>
        /// A text/plain dump of all threads and their stack traces
        /// </summary>
        public ActionResult Threads(string username, string password)
        {
            if (!IsAuthorized(username, password))
            {
                return null;
            }

            var result = new ContentResult
            {
                Content = CLRProfiler.DumpThreads(),
                ContentType = "text/plain",
                ContentEncoding = Encoding.UTF8
            };

            return result;
        }

        private bool IsAuthorized(string username, string password)
        {
            if(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
            {
                // No authorization selected
                return true;
            }

            var auth = ControllerContext.HttpContext.Request.Headers["Authorization"];

            if (String.IsNullOrEmpty(auth))
            {
                return false;
            }

            try
            {
                var encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", ""));
                var value = Encoding.ASCII.GetString(encodedDataAsBytes);
                var userpass = value;
                var user = userpass.Substring(0, userpass.IndexOf(':'));
                var pass = userpass.Substring(userpass.IndexOf(':') + 1);
                
                return user.ToLowerInvariant().Equals(username) && pass.HashWithMd5().Equals(password);;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}