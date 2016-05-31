using System.Data;

namespace tophat
{
    public abstract class ConnectionFactory : IConnectionFactory
    {
        public abstract IDbConnection CreateConnection();
        public IDbConnection GetUnitOfWorkScopedConnection()
        {
            var context = Database.Container.Resolve<DataContext>();
            return context == null ? null : context.Connection;
        }
        public string ConnectionString { get; set; }
    }
}