using System;
using System.Collections.Generic;
using container;

namespace cms
{
    public class CmsResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly Container _container;
        private readonly System.Web.Mvc.IDependencyResolver _fallback;
        
        public CmsResolver(Container container, System.Web.Mvc.IDependencyResolver fallback)
        {
            _container = container;
            _fallback = fallback;
        }
        
        public object GetService(Type serviceType)
        {
            var resolved = _container.Resolve(serviceType);
            return resolved ?? ResolveWithFallback(serviceType);
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