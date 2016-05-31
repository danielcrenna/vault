using System.Data;
using System.Data.SQLite;

namespace tophat.Tests
{
    public class SqliteConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }

    public class SqliteDataContext : DataContext<SqliteConnectionFactory>
    {
        public SqliteDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}