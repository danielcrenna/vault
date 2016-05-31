using System.Data;
using System.Data.SQLite;
using tophat;

namespace linger.Mvc.Demo
{
    internal class SqliteConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}