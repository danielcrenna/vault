using ChainLib.Wallets;
using ChainLib.Wallets.StorageFormats;

namespace ChainLib.Tests.Wallets.Fixtures
{
	public class KeyStoreStorageFormatFixture
	{
		public KeyStoreStorageFormatFixture()
		{
			Value = new KeystoreFileStorageFormat();
		}

		public IWalletAddressStorageFormat Value { get; set; }
	}
}