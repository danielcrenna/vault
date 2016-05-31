using tophat;

namespace bulky.Tests.Sqlite
{
    public class SqliteDataContext : DataContext<SqliteConnectionFactory>
    {
        public SqliteDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}