using System;
using CoinLib.DataAccess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoinLib.DataAccess;
using CoinLib.Extensions;
using CoinLib.Node.Tests.Fixtures;

namespace CoinLib.Tests.Fixtures.DataAccess.Blocks
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
			    transaction.Hash = hashProvider.ComputeHashBytes(transaction);
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