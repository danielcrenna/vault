using System;
using System.Data;

namespace tophat
{
    public class ProxyConnectionFactory : ConnectionFactory
    {
        public Func<string, IDbConnection> Proxy { get; set; }

        public override IDbConnection CreateConnection()
        {
            var connection = Proxy(ConnectionString);
            return connection;
        }
    }
}