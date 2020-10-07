using System;
using Microsoft.Extensions.Logging;
using CoinLib.DataAccess;

namespace CoinLib.Tests.Fixtures.DataAccess.Blocks
{
    public class EmptyTransactionDatabaseFixture : IDisposable
    {
        public SqliteTransactionRepository Value { get; set; }

        public EmptyTransactionDatabaseFixture(string @namespace)
        {
            var factory = new LoggerFactory();
            factory.AddConsole();

            Value = new SqliteTransactionRepository(
				@namespace, 
				"blockchain", 
				factory.CreateLogger<SqliteTransactionRepository>());

	        Value.MigrateToLatest();
        }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}