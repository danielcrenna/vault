namespace NaiveCoin.Wallets.Tests.Fixtures
{
    public class BrainWalletProviderFixture
    {
        public BrainWalletProviderFixture()
        {
            Value = new BrainWalletProvider();
        }

        public IWalletProvider Value { get; set; }
    }
}