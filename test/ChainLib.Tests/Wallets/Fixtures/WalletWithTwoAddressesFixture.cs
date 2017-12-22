using ChainLib.Wallets;
using ChainLib.Wallets.Factories;

namespace ChainLib.Tests.Wallets.Fixtures
{
    public class WalletWithTwoAddressesFixture
    {
        public WalletWithTwoAddressesFixture()
        {
            var factory = new FixedSaltWalletFactoryProvider(Constants.DefaultFixedSalt16);
            var provider = new WalletAddressProviderFixture();
            var wallet = factory.Create("rosebud");

            provider.Value.GenerateAddress(wallet);
            provider.Value.GenerateAddress(wallet);

            Value = wallet;
        }

        public Wallet Value { get; set; }
    }
}