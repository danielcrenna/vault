using ChainLib.Wallets;
using ChainLib.Wallets.Secrets;

namespace ChainLib.Tests.Wallets.Fixtures
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