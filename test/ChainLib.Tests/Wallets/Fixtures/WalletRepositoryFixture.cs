using System;
using ChainLib.Sqlite;
using ChainLib.Wallets.Addresses;
using ChainLib.Wallets.Secrets;
using Microsoft.Extensions.Logging;

namespace ChainLib.Tests.Wallets.Fixtures
{
	public class WalletRepositoryFixture
	{
		public WalletRepositoryFixture()
		{
			var secrets = new PasswordHashSecretProvider();
			var addresses = new DeterministicWalletAddressProvider(secrets);

			var factory = new LoggerFactory();

			var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			Value = new SqliteWalletRepository(
				baseDirectory,
				$"{Guid.NewGuid()}",
				"wallets",
				factory.CreateLogger<SqliteWalletRepository>());
		}

		public SqliteWalletRepository Value { get; set; }
	}
}