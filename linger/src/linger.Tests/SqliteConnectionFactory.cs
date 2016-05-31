using System.Data;
using System.Data.SQLite;
using tophat;

namespace linger.Tests
{
    public class SqliteConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}