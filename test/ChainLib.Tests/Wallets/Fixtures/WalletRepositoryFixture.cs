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
			factory.AddConsole();

			Value = new SqliteWalletRepository(
				$"{Guid.NewGuid()}",
				"wallets",
				addresses,
				factory.CreateLogger<SqliteWalletRepository>());
		}

		public SqliteWalletRepository Value { get; set; }
	}
}