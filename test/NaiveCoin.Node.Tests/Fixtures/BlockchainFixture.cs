using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Models;
using NaiveCoin.Services;
using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;

namespace NaiveCoin.Tests.Fixtures
{
	public class BlockchainFixture
	{
        public BlockchainFixture()
        {
	        var @namespace = $"{Guid.NewGuid()}";

	        var coinSettings = new CoinSettingsFixture();
			var blocks = new EmptyBlockchainFixtureWithNamespace(@namespace);
            var transactions = new EmptyTransactionDatabaseFixture(@namespace);
            var coinSettingsOptions = new OptionsWrapper<CoinSettings>(coinSettings.Value);
			var hashProvider = new HashProviderFixture();
			var jsonSettings = new JsonSerializerSettingsFixture();
	        var pow = new CoinBasedProofOfWork(coinSettingsOptions, hashProvider.Value);

			var factory = new LoggerFactory();
            factory.AddConsole();
            
            Value = new Blockchain(
	            coinSettingsOptions,
                blocks.Value,
                pow,
                transactions.Value,
                hashProvider.Value,
	            jsonSettings.Value, 
				factory.CreateLogger<Blockchain>());
        }

        public Blockchain Value { get; set; }
    }
}