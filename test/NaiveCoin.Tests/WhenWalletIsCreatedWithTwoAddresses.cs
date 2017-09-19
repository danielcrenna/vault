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
    }
}