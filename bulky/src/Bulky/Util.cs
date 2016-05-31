using System.Data;

namespace bulky
{
    public class Util
    {
        public static object AdHoc(IDbConnection connection, IDbTransaction transaction, int timeout, string sql)
        {
            if (connection.State == ConnectionState.Closed) connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            command.Transaction = transaction;
            return command.ExecuteScalar();
        }
    }
}