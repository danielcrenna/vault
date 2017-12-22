using System.IO;
using ChainLib.Crypto;
using ChainLib.Tests.Wallets.Fixtures;
using ChainLib.Wallets;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.StorageFormats;
using Xunit;

namespace ChainLib.Tests.Wallets
{
	public class WhenWalletIsCreatedWithSingleAddress : 
		IClassFixture<WalletWithOneAddressFixture>,
		IClassFixture<KeyStoreStorageFormatFixture>
	{
		private readonly KeyStoreStorageFormatFixture _keystore;

		public WhenWalletIsCreatedWithSingleAddress(
			WalletWithOneAddressFixture fixture,
			KeyStoreStorageFormatFixture keystore)
		{
			_keystore = keystore;
			Fixture = fixture;
		}

		public WalletWithOneAddressFixture Fixture { get; set; }

		[Fact]
		public void There_is_one_keypair_in_the_wallet()
		{
			Assert.NotNull(Fixture.Value);
			Assert.Equal(1, Fixture.Value.KeyPairs.Count);
		}

		[Fact]
		public void The_wallet_can_be_exported_to_keystore_on_disk()
		{
			var wallet1 = Fixture.Value;
			var filename = KeystoreFileStorageFormat.WriteToFile(Path.GetTempPath(), wallet1);
			var keystore = File.ReadAllText(filename);

			var factory = new FixedSaltWalletFactoryProvider(Constants.DefaultFixedSalt16);
			var wallet2 = factory.Create("rosebud");
			_keystore.Value.Import(wallet2, keystore, Constants.KeystoreKeyLength);

			Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
			Assert.Equal(wallet1.KeyPairs[0].PublicKey.ToHex(), wallet2.KeyPairs[0].PublicKey.ToHex());
			Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), wallet2.KeyPairs[0].PrivateKey.ToHex());
		}
	}
}