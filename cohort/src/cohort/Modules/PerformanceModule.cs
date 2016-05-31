using System.Web;
using minirack;

namespace cohort.Modules
{
    /// <summary>
    /// Provides dynamic performance optimization when called for
    /// <remarks>
    /// The philosophy here is, CPU is probably not your bottleneck, so make use of it to alleviate pain.
    /// The pain in this case is managing static resource builds and IIS configuration.
    /// </remarks>
    /// </summary>
    [Pipeline]
    public class PerformanceModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            
        }

        public void Dispose()
        {

        }
    }
}