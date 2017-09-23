namespace NaiveCoin.Wallets.Tests.Fixtures
{
    public class SecureWalletProviderFixture
    {
        public SecureWalletProviderFixture()
        {
            Value = new SecureWalletProvider();
        }

        public IWalletProvider Value { get; set; }
    }
}