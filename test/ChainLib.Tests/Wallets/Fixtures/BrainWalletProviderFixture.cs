using ChainLib.Wallets;
using ChainLib.Wallets.Providers;

namespace ChainLib.Tests.Wallets.Fixtures
{
    public class BrainWalletProviderFixture
    {
        public BrainWalletProviderFixture()
        {
			var repository = new WalletRepositoryFixture();

            Value = new BrainWalletProvider(repository.Value);
        }

        public IWalletProvider Value { get; set; }
    }
}