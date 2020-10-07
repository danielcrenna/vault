using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoinLib.Node.Tests.Fixtures;
using CoinLib.Services;
using Xunit;

namespace CoinLib.Tests.Fixtures
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