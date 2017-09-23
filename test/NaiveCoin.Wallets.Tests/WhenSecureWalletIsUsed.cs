using NaiveCoin.Core.Helpers;
using NaiveCoin.Wallets.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Wallets.Tests
{
    public class WhenSecureWalletIsUsed : IClassFixture<SecureWalletProviderFixture>
    {
        private readonly SecureWalletProviderFixture _provider;

        public WhenSecureWalletIsUsed(SecureWalletProviderFixture provider)
        {
            _provider = provider;
        }

        [Theory]
        [InlineData("purple monkey dishwasher")]
        public void The_wallet_password_hash_is_verifiable(string passphrase)
        {
            Wallet wallet1 = _provider.Value.Create(passphrase);
            Wallet wallet2 = _provider.Value.Create(passphrase);

            Assert.True(CryptoUtil.VerifyPassword(passphrase, wallet1.PasswordHash));
            Assert.True(CryptoUtil.VerifyPassword(passphrase, wallet2.PasswordHash));
        }
    }
}