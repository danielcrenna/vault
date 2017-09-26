using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
	        var blockObjectSerializer = new BlockObjectDataSerializerFixture().Value;

            var factory = new LoggerFactory();
            factory.AddConsole();
            
            Value = new SqliteCurrencyBlockRepository(
				$"{Guid.NewGuid()}", 
				"blockchain",
				new OptionsWrapper<CoinSettings>(coinSettings), 
				hashProvider,
				blockObjectSerializer,
				factory.CreateLogger<SqliteCurrencyBlockRepository>());

            var block = coinSettings.GenesisBlock;
            foreach (var transaction in block.Transactions)
                transaction.Hash = transaction.ToHash(hashProvider);
            block.Hash = block.ToHash(hashProvider);
            
            Value.AddAsync(block).ConfigureAwait(false).GetAwaiter().GetResult(); ;
        }

        public SqliteCurrencyBlockRepository Value { get; set; }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}