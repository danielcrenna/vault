using System;
using System.Collections.Generic;

namespace container
{
    public class Container : IContainer
    {
        private readonly IDependencyResolver _resolver;
        private readonly IDependencyRegistrar _registrar;
        public static Func<IContainer> DefaultContainer { get; set; }
        static Container()
        {
            DefaultContainer = ()=> new DefaultContainer();
        }
        public Container(IDependencyResolver resolver, IDependencyRegistrar registrar)
        {
            _resolver = resolver;
            _registrar = registrar;
        }
        public Container()
        {
            var container = DefaultContainer();
            _resolver = container;
            _registrar = container;
        }
        public bool CanResolve<T>() where T : class
        {
            return _resolver.CanResolve<T>();
        }
        public T Resolve<T>() where T : class
        {
            return _resolver.Resolve<T>();
        }
        public T Resolve<T>(string name) where T : class
        {
            return _resolver.Resolve<T>(name);
        }
        public bool CanResolve(Type serviceType)
        {
            return _resolver.CanResolve(serviceType);
        }
        public object Resolve(Type serviceType)
        {
            return _resolver.Resolve(serviceType);
        }
        public object Resolve(Type serviceType, string name)
        {
            return _resolver.Resolve(serviceType, name);
        }
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _resolver.ResolveAll<T>();
        }
        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _resolver.ResolveAll(serviceType);
        }
        public ILifetime Register<T>(Func<T> builder) where T : class
        {
            return _registrar.Register(builder);
        }
        public ILifetime Register<T>(string name, Func<T> builder) where T : class
        {
            return _registrar.Register(name, builder);
        }
        public ILifetime Register<T>(Func<IDependencyResolver, T> builder) where T : class
        {
            return _registrar.Register(r => builder(this));
        }
        public ILifetime Register<T>(string name, Func<IDependencyResolver, T> builder) where T : class
        {
            return _registrar.Register(name, builder);
        }
        public ILifetime Register(Type type, Func<IDependencyResolver, object> builder)
        {
            return _registrar.Register(type, builder);
        }
        public bool Remove<T>() where T : class
        {
            return _registrar.Remove<T>();
        }
        public bool Remove<T>(string name) where T : class
        {
            return _registrar.Remove<T>(name);
        }
        public void Dispose()
        {
            _resolver.Dispose();
            if (_registrar != _resolver)
            {
                _registrar.Dispose();
            }
        }
    }
}