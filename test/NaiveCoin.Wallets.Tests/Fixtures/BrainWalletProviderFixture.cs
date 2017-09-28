namespace NaiveCoin.Wallets.Tests.Fixtures
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