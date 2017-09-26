using System;
using Microsoft.Extensions.Logging;
using NaiveCoin.DataAccess;

namespace NaiveCoin.Tests.Fixtures.DataAccess.Blocks
{
    public class EmptyTransactionDatabaseFixture : IDisposable
    {
        public SqliteTransactionRepository Value { get; set; }

        public EmptyTransactionDatabaseFixture()
        {
            var factory = new LoggerFactory();
            factory.AddConsole();

            Value = new SqliteTransactionRepository(
				$"{Guid.NewGuid()}", 
				"blockchain", 
				factory.CreateLogger<SqliteTransactionRepository>());
        }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}