using System;
using Microsoft.Extensions.Logging;
using NaiveCoin.DataAccess;
using NaiveCoin.Extensions;

namespace NaiveCoin.Tests.Fixtures.DataAccess.Blocks
{
    public class BlockDatabaseWithGenesisBlockFixture : IDisposable
    {
        public BlockDatabaseWithGenesisBlockFixture()
        {
            var coinSettings = new CoinSettingsFixture().Value;
            var hashProvider = new ObjectHashProviderFixture().Value;
            
            var factory = new LoggerFactory();
            factory.AddConsole();
            
            Value = new SqliteBlockRepository($"{Guid.NewGuid()}", "blockchain", factory.CreateLogger<SqliteBlockRepository>());

            var block = coinSettings.GenesisBlock;
            foreach (var transaction in block.Transactions)
                transaction.Hash = transaction.ToHash(hashProvider);
            block.Hash = block.ToHash(hashProvider);
            
            Value.Add(block);
        }

        public SqliteBlockRepository Value { get; set; }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}