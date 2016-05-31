using System.Data;
using System.Data.SqlClient;

namespace tophat
{
    public class SqlServerConnectionFactory : ConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}