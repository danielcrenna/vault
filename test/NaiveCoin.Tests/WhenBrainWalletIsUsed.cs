using NaiveCoin.Models;
using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
{
    public class WhenBrainWalletIsUsed : IClassFixture<WalletProviderFixture>
    {
        public WhenBrainWalletIsUsed(WalletProviderFixture fixture)
        {
            Fixture = fixture;
        }

        public WalletProviderFixture Fixture { get; set; }

        [Fact]
        public void The_wallet_can_be_deterministically_recreated_by_passphrase()
        {
            Wallet wallet1 = Fixture.Value.CreateFromPassword("purple monkey dishwasher");
            Fixture.Value.GenerateAddress(wallet1);
            Fixture.Value.GenerateAddress(wallet1);

            Wallet wallet2 = Fixture.Value.CreateFromPassword("purple monkey dishwasher");
            Fixture.Value.GenerateAddress(wallet2);
            Fixture.Value.GenerateAddress(wallet2);

            Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
            Assert.Equal(wallet1.KeyPairs[0], wallet2.KeyPairs[0]);
            Assert.Equal(wallet1.KeyPairs[1], wallet2.KeyPairs[1]);
        }
    }
}