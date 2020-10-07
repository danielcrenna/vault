using ChainLib.Wallets;
using ChainLib.Wallets.Addresses;

namespace ChainLib.Tests.Wallets.Fixtures
{
    public class WalletAddressProviderFixture
    {
        public WalletAddressProviderFixture()
        {
            var secrets = new WalletSecretProviderFixture();

            Value = new DeterministicWalletAddressProvider(secrets.Value);
        }

        public IWalletAddressProvider Value { get; set; }
    }
}