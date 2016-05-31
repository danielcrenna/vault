using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using container;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace cohort.Api.Configuration
{
    public class CohortDependencyScope : IDependencyScope
    {
        private readonly Container _container;
        private readonly IDependencyResolver _fallback;
        public CohortDependencyScope(Container container, IDependencyResolver fallback)
        {
            _container = container;
            _fallback = fallback;
        }
        public void Dispose()
        {
            // Container doesn't have the concept of a child and I'm not sure there's any benefit to cloning it for this purpose
            // See: http://byterot.blogspot.ca/2012/04/aspnet-web-api-series-part-4-dependency.html
        }
        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType) ?? ResolveWithFallback(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveAll(serviceType) ?? ResolveAllWithFallback(serviceType);
        }
        private object ResolveWithFallback(Type serviceType)
        {
            var fallback = _fallback.GetService(serviceType);
            return fallback;
        }
        private IEnumerable<object> ResolveAllWithFallback(Type serviceType)
        {
            var fallback = _fallback.GetServices(serviceType);
            return fallback;
        }
    }
}