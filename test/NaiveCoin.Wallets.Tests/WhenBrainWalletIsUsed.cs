using NaiveCoin.Wallets.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Wallets.Tests
{
    public class WhenBrainWalletIsUsed : IClassFixture<BrainWalletProviderFixture>
    {
        private readonly BrainWalletProviderFixture _provider;

        public WhenBrainWalletIsUsed(BrainWalletProviderFixture fixture)
        {
            _provider = fixture;
        }

        [Fact]
        public void The_wallet_can_be_deterministically_recreated_by_passphrase()
        {
            Wallet wallet1 = _provider.Value.Create("purple monkey dishwasher");
            _provider.Value.GenerateAddress(wallet1);
            _provider.Value.GenerateAddress(wallet1);

            Wallet wallet2 = _provider.Value.Create("purple monkey dishwasher");
            _provider.Value.GenerateAddress(wallet2);
            _provider.Value.GenerateAddress(wallet2);

            Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
            Assert.Equal(wallet1.KeyPairs[0], wallet2.KeyPairs[0]);
            Assert.Equal(wallet1.KeyPairs[1], wallet2.KeyPairs[1]);
        }
    }
}