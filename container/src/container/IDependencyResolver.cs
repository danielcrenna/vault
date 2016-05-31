using System;
using System.Collections.Generic;

namespace container
{
    public interface IDependencyResolver : IDisposable
    {
        bool CanResolve<T>() where T : class;
        T Resolve<T>() where T : class;
        T Resolve<T>(string name) where T : class;
        bool CanResolve(Type serviceType);
        object Resolve(Type serviceType);
        object Resolve(Type serviceType, string name);
        IEnumerable<T> ResolveAll<T>() where T : class;
        IEnumerable<object> ResolveAll(Type serviceType);
    }
}
