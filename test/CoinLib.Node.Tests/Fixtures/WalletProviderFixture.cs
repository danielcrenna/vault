using CoinLib.Wallets;

namespace CoinLib.Tests.Fixtures
{
	public class WalletProviderFixture
	{
		public WalletProviderFixture()
		{
			var repository = new WalletRepositoryFixture();

			Value = new BrainWalletProvider(repository.Value);
		}

		public BrainWalletProvider Value { get; set; }
	}
}