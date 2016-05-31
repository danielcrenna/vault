using tophat;

namespace bulky.Tests.MySql
{
    public class MySqlDataContext : DataContext<MySqlConnectionFactory>
    {
        public MySqlDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}