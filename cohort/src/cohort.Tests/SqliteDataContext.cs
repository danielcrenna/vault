using tophat;

namespace cohort.Tests
{
    public class SqliteDataContext : DataContext<SqliteConnectionFactory>
    {
        public SqliteDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}