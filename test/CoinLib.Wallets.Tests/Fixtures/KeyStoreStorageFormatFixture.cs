namespace CoinLib.Wallets.Tests.Fixtures
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