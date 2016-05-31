using System.Web.Http.Dependencies;
using container;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace cohort.Api.Configuration
{
    public class CohortDependencyResolver : CohortDependencyScope, IDependencyResolver
    {
        public CohortDependencyResolver(Container container, IDependencyResolver fallback) : base(container, fallback)
        {
            // Scopeless
        }
        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}