using System;
using Microsoft.Extensions.Logging;
using NaiveCoin.Wallets.DataAccess;

namespace NaiveCoin.Wallets.Tests.Fixtures
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