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

            var serializer = new TransactionDataSerializerFixture();

            Value = new SqliteTransactionRepository($"{Guid.NewGuid()}", serializer.Value, factory.CreateLogger<SqliteTransactionRepository>());
        }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}