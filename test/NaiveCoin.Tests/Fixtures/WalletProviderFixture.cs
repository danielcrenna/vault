using NaiveCoin.Wallets;

namespace NaiveCoin.Tests.Fixtures
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