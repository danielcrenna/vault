using System.Web;
using cohort.Logging;
using minirack;

namespace cohort.Modules
{
    /// <summary>
    /// Flushes the log buffer at the end of a request, so only one write to the DB is performed for logging
    /// </summary>
    [Pipeline]
    public class LoggingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += (sender, args) => CohortLogTarget.Flush();
        }

        public void Dispose()
        {
            
        }
    }
}