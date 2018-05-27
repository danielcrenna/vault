using ChainLib.Wallets;
using ChainLib.Wallets.Providers;

namespace ChainLib.Tests.Wallets.Fixtures
{
    public class RandomWalletProviderFixture
    {
        public RandomWalletProviderFixture()
        {
			var repository = new WalletRepositoryFixture();

            Value = new RandomWalletProvider(repository.Value);
        }

        public IWalletProvider Value { get; set; }
    }
}