using System;
using System.Web.Mvc;
using System.Web.Routing;
using metrics.AspNetMvc.Extensions;

namespace metrics.AspNetMvc
{
    /// <summary>
    /// A wrapper for enabling default metrics API routes in an ASP.NET MVC application
    /// </summary>
    public static class Metrics
    {
        /// <summary>
        /// The URI path to use when registering the 'metrics' API method route
        /// </summary>
        public static string MetricsPath { get; set; }

        /// <summary>
        /// The URI path to use when registering the 'ping' API method route
        /// </summary>
        public static string PingPath { get; set; }

        /// <summary>
        /// The URI path to use when registering the 'healthcheck' API method route
        /// </summary>
        public static string HealthCheckPath { get; set; }
        
        /// <summary>
        /// The URI path to use when registering the 'threads' API method route
        /// </summary>
        public static string ThreadsPath { get; set; }

        static Metrics()
        {
            MetricsPath = "metrics";
            PingPath = "ping";
            HealthCheckPath = "healthcheck";
            ThreadsPath = "threads";
        }

        /// <summary>
        /// Registers default routes for metric methods, without authorization:
        /// GET /metrics: A JSON object of all registered metrics and a host of CLR metrics
        /// GET /ping: A simple text/plain "pong" for load-balancers
        /// GET /healthcheck: Runs through all registered HealthCheck instances and reports the results; 
        ///                   returns a `200 OK` if all succeeded, or a `500 Internal Server Error` if any failed
        /// GET /threads: A text/plain dump of all threads and their stack traces
        /// </summary>
        public static void RegisterRoutes()
        {
            MapRoutes(RouteTable.Routes, null, null);
        }

        /// <summary>
        /// Registers default routes for metric methods, without HTTP Basic authorization:
        /// GET /metrics: A JSON object of all registered metrics and a host of CLR metrics
        /// GET /ping: A simple text/plain "pong" for load-balancers
        /// GET /healthcheck: Runs through all registered HealthCheck instances and reports the results; 
        ///                   returns a `200 OK` if all succeeded, or a `500 Internal Server Error` if any failed
        /// GET /threads: A text/plain dump of all threads and their stack traces
        /// </summary>
        /// <param name="username">The desired username</param>
        /// <param name="password">The desired password</param>
        public static void RegisterRoutes(string username, string password)
        {
            if(string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("You must provide a username", "username");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("You must provide a password", "password");
            }

            MapRoutes(RouteTable.Routes, username, password.HashWithMd5());
        }

        private static void MapRoutes(RouteCollection routes, string username, string password)
        {
            routes.MapRoute("metrics-net-ping", PingPath, new { controller = "Metrics", action = "Ping", username, password }, new[] { "metrics.AspNetMvc" });
            routes.MapRoute("metrics-net-metrics", MetricsPath, new { controller = "Metrics", action = "Metrics", username, password }, new[] { "metrics.AspNetMvc" });
            routes.MapRoute("metrics-net-healthcheck", HealthCheckPath, new { controller = "Metrics", action = "HealthCheck", username, password }, new[] { "metrics.AspNetMvc" });
            routes.MapRoute("metrics-net-threads", ThreadsPath, new { controller = "Metrics", action = "Threads", username, password }, new[] { "metrics.AspNetMvc" });
        }
    }
}