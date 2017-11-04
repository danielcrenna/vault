namespace CoinLib.Wallets.Tests.Fixtures
{
    public class SecureWalletProviderFixture
    {
        public SecureWalletProviderFixture()
        {
			var repository = new WalletRepositoryFixture();

            Value = new SecureWalletProvider(repository.Value);
        }

        public IWalletProvider Value { get; set; }
    }
}