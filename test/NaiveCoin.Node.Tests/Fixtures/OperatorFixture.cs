using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Node.Tests.Fixtures;
using NaiveCoin.Services;
using Xunit;

namespace NaiveCoin.Tests.Fixtures
{
	[Collection("Blockchain")]
	public class OperatorFixture
	{
		public OperatorFixture(BlockchainFixture blockchain)
		{
			var coinSettings = new CoinSettingsFixture();
			var hashProvider = new HashProviderFixture();
			var walletProvider = new WalletProviderFixture();

			var factory = new LoggerFactory();
			factory.AddConsole();

			Value = new Operator(blockchain.Value, hashProvider.Value, walletProvider.Value,
				new OptionsWrapper<CoinSettings>(coinSettings.Value), factory.CreateLogger<Operator>());
		}

		public Operator Value { get; set; }
	}
}