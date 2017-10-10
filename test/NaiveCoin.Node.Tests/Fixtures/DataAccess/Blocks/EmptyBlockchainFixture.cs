using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.DataAccess;
using NaiveCoin.Extensions;
using NaiveCoin.Node.Tests.Fixtures;

namespace NaiveCoin.Tests.Fixtures.DataAccess.Blocks
{
	public class EmptyBlockchainFixture : IDisposable
    {
        public EmptyBlockchainFixture()
        {
	        var @namespace = $"{Guid.NewGuid()}";

	        Init(@namespace);
        }

	    protected void Init(string @namespace)
	    {
		    var coinSettings = new CoinSettingsFixture().Value;
		    var hashProvider = new HashProviderFixture().Value;
		    var factory = new LoggerFactory();
		    factory.AddConsole();

		    Value = new SqliteCurrencyBlockRepository(
			    @namespace,
			    "blockchain",
			    new OptionsWrapper<CoinSettings>(coinSettings),
			    hashProvider,
			    factory.CreateLogger<SqliteCurrencyBlockRepository>());

		    var block = coinSettings.GenesisBlock;
		    foreach (var transaction in block.Transactions)
			    transaction.Hash = transaction.ToHash(hashProvider);
		    block.Hash = block.ToHashBytes(hashProvider);

		    Value.AddAsync(block).ConfigureAwait(false).GetAwaiter().GetResult();
	    }

	    public SqliteCurrencyBlockRepository Value { get; set; }

        public void Dispose()
        {
            Value.Purge();
        }
    }
}