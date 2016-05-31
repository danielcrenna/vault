using System;
using System.Data;

namespace tophat
{
    public class DataContext<T> : DataContext, IDbConnection where T : IConnectionFactory, new()
    {
        public DataContext(string connectionString) : base(new T { ConnectionString = connectionString })
        {
            PrimeConnection();
        }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; private set; }
        public string Database { get; private set; }
        public ConnectionState State { get; private set; }

        #region Passthrough
        public IDbTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return Connection.BeginTransaction(il);
        }

        public void Close()
        {
            Connection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public void Open()
        {
            Connection.Open();
        }
        #endregion
    }

    public class DataContext : IDisposable
    {
        private static readonly object Sync = new object();
        private volatile IDbConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        public DataContext(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public IDbConnection Connection { get { return GetConnection(); } }
        private IDbConnection GetConnection()
        {
            PrimeConnection();
            return _connection;
        }
        protected void PrimeConnection()
        {
            if (_connection != null) return;
            lock (Sync)
            {
                if (_connection != null) return;
                var connection = _connectionFactory.CreateConnection();
                connection.Open();
                _connection = connection;
            }
        }
        public void Dispose()
        {
            if (_connection == null)
            {
                return;
            }
            _connection.Dispose();
            _connection = null;
        }
    }
}