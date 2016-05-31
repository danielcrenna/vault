using System.Data;
using MySql.Data.MySqlClient;
using tophat;

namespace bulky.Tests.MySql
{
    public class MySqlConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
