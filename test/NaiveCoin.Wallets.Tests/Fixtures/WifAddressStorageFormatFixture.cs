namespace NaiveCoin.Wallets.Tests.Fixtures
{
    public class WifAddressStorageFormatFixture
    {
        public WifAddressStorageFormatFixture()
        {
            Value = new WifAddressStorageFormat();
        }

        public IWalletAddressStorageFormat Value { get; set; }
    }
}