namespace NaiveCoin.Wallets.Tests.Fixtures
{
	public class WalletWithOneAddressFixture
	{
		public WalletWithOneAddressFixture()
		{
			var factory = new FixedSaltWalletFactoryProvider("_NaiveCoin_Salt_");
			var provider = new WalletAddressProviderFixture();
			var wallet = factory.Create("rosebud");

			provider.Value.GenerateAddress(wallet);

			Value = wallet;
		}

		public Wallet Value { get; set; }
	}
}