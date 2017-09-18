using NaiveCoin.Models;
using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
{
    public class WhenWalletIsCreatedWithTwoAddresses : IClassFixture<WalletWithTwoAddressesFixture>
    {
        public WhenWalletIsCreatedWithTwoAddresses(WalletWithTwoAddressesFixture fixture)
        {
            Fixture = fixture;
        }

        public WalletWithTwoAddressesFixture Fixture { get; set; }

        [Fact]
        public void There_are_two_keypairs_in_the_wallet()
        {
            Assert.NotNull(Fixture.Value);
            Assert.Equal(2, Fixture.Value.KeyPairs.Count);
        }

        [Fact]
        public void The_wallet_can_be_deterministically_recreated_by_passphrase()
        {
            var recovered = Wallet.CreateFromPassword("rosebud");
            recovered.GenerateAddress();
            recovered.GenerateAddress();

            Assert.Equal(recovered.KeyPairs.Count, Fixture.Value.KeyPairs.Count);
            Assert.Equal(recovered.KeyPairs[0], Fixture.Value.KeyPairs[0]);
            Assert.Equal(recovered.KeyPairs[1], Fixture.Value.KeyPairs[1]);
        }
    }
}