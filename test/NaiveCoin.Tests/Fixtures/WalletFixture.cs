using NaiveCoin.Models;

namespace NaiveCoin.Tests.Fixtures
{
    public class WalletWithTwoAddressesFixture
    {
        public WalletWithTwoAddressesFixture()
        {
            var wallet = Wallet.CreateFromPassword("rosebud");

            wallet.GenerateAddress();
            wallet.GenerateAddress();

            Value = wallet;
        }

        public Wallet Value { get; set; }
    }
}