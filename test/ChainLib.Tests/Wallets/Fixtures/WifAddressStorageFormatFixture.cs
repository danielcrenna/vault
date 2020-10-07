using ChainLib.Wallets;
using ChainLib.Wallets.StorageFormats;

namespace ChainLib.Tests.Wallets.Fixtures
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