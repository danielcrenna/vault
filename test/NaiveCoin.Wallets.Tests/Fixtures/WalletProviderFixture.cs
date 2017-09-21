namespace NaiveCoin.Wallets.Tests.Fixtures
{
    public class WalletProviderFixture
    {
        public WalletProviderFixture()
        {
            Value = new BrainWalletProvider();
        }

        public IWalletProvider Value { get; set; }
    }
}