using System.Data;

namespace tophat
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; set; }
        IDbConnection CreateConnection();
        IDbConnection GetUnitOfWorkScopedConnection();
    }
}