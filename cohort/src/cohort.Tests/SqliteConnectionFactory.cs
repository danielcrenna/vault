using System.Data;
using System.Data.SQLite;
using tophat;

namespace cohort.Tests
{
    public class SqliteConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}