namespace tophat
{
    public class SqlServerDataContext : DataContext<SqlServerConnectionFactory>
    {
        public SqlServerDataContext(string connectionString) : base(connectionString)
        {

        }
    }
}