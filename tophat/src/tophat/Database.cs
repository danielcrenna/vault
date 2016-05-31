using System;
using System.Data;
using System.Diagnostics;
using container;

namespace tophat
{
    public static class Database
    {
        public static Container Container;
        static Database()
        {
            Container = new Container();
        }
        public static void Install<T>(string connectionString, ConnectionScope scope = ConnectionScope.ByRequest) where T : class, IConnectionFactory, new()
        {
            ResetContexts();
            var lifetime = Container.Register(r => new DataContext(r.Resolve<IConnectionFactory>()));
            ScopeToLifetime(scope, lifetime);
            RegisterConnectionFactory<T>(scope, connectionString);
        }
        public static void Install<T>(string connectionString, Func<IConnectionFactory, DataContext> scope) where T : class, IConnectionFactory, new()
        {
            ResetContexts();
            Container.Register(r => scope(r.Resolve<IConnectionFactory>()));
            RegisterConnectionFactory<T>(ConnectionScope.AlwaysNew, connectionString);
        }
        public static void Install(string connectionString, Func<string, IDbConnection> proxy, ConnectionScope scope = ConnectionScope.ByRequest)
        {
            ResetContexts();
            var lifetime = Container.Register(r => new DataContext(r.Resolve<IConnectionFactory>()));
            ScopeToLifetime(scope, lifetime);
            RegisterConnectionFactory(scope, connectionString, proxy);
        }
        public static void Install(string connectionString, Func<string, IDbConnection> proxy, Func<IConnectionFactory, DataContext> scope)
        {
            ResetContexts();
            Container.Register(r => scope(r.Resolve<IConnectionFactory>()));
            RegisterConnectionFactory(ConnectionScope.AlwaysNew, connectionString, proxy);
        }
        private static void ResetContexts()
        {
            UnitOfWork.Purge();
            Container.Remove<DataContext>();
            Container.Remove<IConnectionFactory>();
        }
        private static void RegisterConnectionFactory<T>(ConnectionScope scope, string connectionString) where T : class, IConnectionFactory, new()
        {
            var lifetime = Container.Register<IConnectionFactory>(r => new T { ConnectionString = connectionString });
            ScopeToLifetime(scope, lifetime);
            Debug.Assert(Container.Resolve<IConnectionFactory>() != null, "Database registration did not resolve. Check your ConnectionScope, it must not be able to return null in the current context.");
        }
        private static void RegisterConnectionFactory(ConnectionScope scope, string connectionString, Func<string, IDbConnection> proxy)
        {
            var lifetime = Container.Register<IConnectionFactory>(r => new ProxyConnectionFactory { Proxy = proxy, ConnectionString = connectionString});
            ScopeToLifetime(scope, lifetime);
            Debug.Assert(Container.Resolve<IConnectionFactory>() != null, "Database registration did not resolve. Check your ConnectionScope, it must not be able to return null in the current context.");
        }
        private static void ScopeToLifetime(ConnectionScope scope, ILifetime lifetime)
        {
            switch (scope)
            {
                case ConnectionScope.ByRequest:
                    lifetime.Request();
                    break;
                case ConnectionScope.ByThread:
                    lifetime.Thread();
                    break;
                case ConnectionScope.BySession:
                    lifetime.Session();
                    break;
                case ConnectionScope.KeepAlive:
                    lifetime.Permanent();
                    break;
                case ConnectionScope.AlwaysNew:
                    lifetime.AlwaysNew();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scope");
            }
        }
    }
}