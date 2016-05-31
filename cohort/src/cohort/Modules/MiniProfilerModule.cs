using System.Web;
using StackExchange.Profiling;
using minirack;

namespace cohort.Modules
{
    /// <summary>
    /// Enables MiniProfiler if browsing locally
    /// </summary>
    [Pipeline]
    public class MiniProfilerModule : IHttpModule 
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, args) =>
            {
                if (context.Request.IsLocal && !context.Request.Path.StartsWith("/signalr"))
                {
                    MiniProfiler.Start();
                }
            };
            context.EndRequest += (sender, args) => MiniProfiler.Stop();
        }

        public void Dispose()
        {
            
        }
    }
}