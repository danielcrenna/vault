using System;
using System.Data;

namespace tophat
{
    public static class UnitOfWork
    {
        private static Lazy<IConnectionFactory> _factory;
        private static Lazy<IConnectionFactory> CreateFactoryMethod()
        {
            return new Lazy<IConnectionFactory>(() =>
            {
                var factory = Database.Container.Resolve<IConnectionFactory>();
                if (factory == null)
                {
                    throw new NullReferenceException("Unit of work was requested but no database is installed");
                }
                return factory;
            });
        }
        public static IConnectionFactory ConnectionFactory
        {
            get { return _factory.Value; }
        }
        static UnitOfWork()
        {
            Initialize();
        }
        private static void Initialize()
        {
            _factory = CreateFactoryMethod();
            Adapter = @default => @default;
        }
        public static IDbConnection Current
        {
            get { return Adapter(ConnectionFactory.GetUnitOfWorkScopedConnection()); }
        }
        public static Func<IDbConnection, IDbConnection> Adapter { get; set; }
        public static void Purge()
        {
            Initialize();
        }
    }
}