using NaiveCoin.Core.Helpers;
using NaiveCoin.Wallets.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Wallets.Tests
{
	public class WhenWalletIsCreatedWithTwoAddresses :
		IClassFixture<WalletWithTwoAddressesFixture>,
		IClassFixture<WifAddressStorageFormatFixture>
	{
		private readonly WifAddressStorageFormatFixture _wif;

		public WhenWalletIsCreatedWithTwoAddresses(
			WalletWithTwoAddressesFixture fixture,
			WifAddressStorageFormatFixture wif)
		{
			_wif = wif;
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
		public void Both_addresses_can_be_exported_via_WIF()
		{
			var wallet1 = Fixture.Value;
			var wif1 = _wif.Value.Export(wallet1, wallet1.KeyPairs[0].PublicKey);
			var wif2 = _wif.Value.Export(wallet1, wallet1.KeyPairs[1].PublicKey);

			Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), WifAddressStorageFormat.GetPrivateKeyFromImport(wif1).ToHex());
			Assert.Equal(wallet1.KeyPairs[1].PrivateKey.ToHex(), WifAddressStorageFormat.GetPrivateKeyFromImport(wif2).ToHex());
		}

		[Fact]
		public void Both_addresses_can_be_imported_via_WIF()
		{
			var wallet1 = Fixture.Value;
			var wif1 = _wif.Value.Export(wallet1, wallet1.KeyPairs[0].PublicKey);
			var wif2 = _wif.Value.Export(wallet1, wallet1.KeyPairs[1].PublicKey);

			var factory = new FixedSaltWalletFactoryProvider("salt");
			var wallet2 = factory.Create("rosebud");
			_wif.Value.Import(wallet2, wif1);
			_wif.Value.Import(wallet2, wif2);

			Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
			Assert.Equal(wallet1.KeyPairs[0].PublicKey.ToHex(), wallet2.KeyPairs[0].PublicKey.ToHex());
			Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), wallet2.KeyPairs[0].PrivateKey.ToHex());
			Assert.Equal(wallet1.KeyPairs[1].PublicKey.ToHex(), wallet2.KeyPairs[1].PublicKey.ToHex());
			Assert.Equal(wallet1.KeyPairs[1].PrivateKey.ToHex(), wallet2.KeyPairs[1].PrivateKey.ToHex());
		}
	}
}