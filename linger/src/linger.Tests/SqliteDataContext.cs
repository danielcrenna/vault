using tophat;

namespace linger.Tests.Sqlite
{
    public class SqliteDataContext : DataContext<SqliteConnectionFactory>
    {
        public SqliteDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}