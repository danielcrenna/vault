using System;
using System.Collections.Generic;
using Munq;

namespace container
{
    public class DefaultContainer : IContainer
    {
        private readonly IocContainer _container;
        public DefaultContainer()
        {
            _container = new IocContainer();
        }
        public bool CanResolve<T>() where T : class
        {
            return _container.CanResolve<T>();
        }
        public T Resolve<T>() where T : class
        {
            return _container.CanResolve<T>() ? _container.Resolve<T>() : null;
        }
        public T Resolve<T>(string name) where T : class
        {
            return _container.Resolve<T>(name);
        }
        public bool CanResolve(Type serviceType)
        {
            return _container.CanResolve(serviceType);
        }
        public object Resolve(Type serviceType)
        {
            return _container.CanResolve(serviceType) ? _container.Resolve(serviceType) : null;
        }
        public object Resolve(Type serviceType, string name)
        {
            return _container.Resolve(name, serviceType);
        }
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _container.ResolveAll<T>();
        }
        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }
        public ILifetime Register<T>(Func<T> builder) where T : class
        {
            return new DefaultLifetime(_container.Register(r => builder()));
        }
        public ILifetime Register<T>(string name, Func<T> builder) where T : class
        {
            return new DefaultLifetime(_container.Register(name, r => builder()));
        }
        public ILifetime Register<T>(Func<IDependencyResolver, T> builder) where T : class
        {
            return new DefaultLifetime(_container.Register(r => builder(this)));
        }
        public ILifetime Register<T>(string name, Func<IDependencyResolver, T> builder) where T : class
        {
            return new DefaultLifetime(_container.Register(name, r => builder(this)));
        }
        public ILifetime Register(Type type, Func<IDependencyResolver, object> builder)
        {
            return new DefaultLifetime(_container.Register(type, r => builder(this)));
        }
        public bool Remove<T>() where T : class
        {
            var registration = _container.GetRegistration<T>();
            if (registration == null) return false;
            _container.Remove(registration);
            return true;
        }
        public bool Remove<T>(string name) where T : class
        {
            var registration = _container.GetRegistration<T>(name);
            if (registration == null) return false;
            _container.Remove(registration);
            return true;
        }
        public void Dispose()
        {
            _container.Dispose();
        }
    }
}