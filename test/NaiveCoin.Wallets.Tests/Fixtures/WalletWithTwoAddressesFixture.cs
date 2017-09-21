namespace NaiveCoin.Wallets.Tests.Fixtures
{
    public class WalletWithTwoAddressesFixture
    {
        public WalletWithTwoAddressesFixture()
        {
            var provider = new WalletProviderFixture();

            var wallet = provider.Value.CreateFromPassword("rosebud");

            provider.Value.GenerateAddress(wallet);
            provider.Value.GenerateAddress(wallet);

            Value = wallet;
        }

        public Wallet Value { get; set; }
    }
}