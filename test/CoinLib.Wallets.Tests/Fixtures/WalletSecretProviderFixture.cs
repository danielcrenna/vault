namespace CoinLib.Wallets.Tests.Fixtures
{
    public class WalletSecretProviderFixture
    {
        public WalletSecretProviderFixture()
        {
            Value = new PasswordHashSecretProvider();
        }

        public IWalletSecretProvider Value { get; set; }
    }
}