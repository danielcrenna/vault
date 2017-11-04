using System;
using Microsoft.Extensions.Logging;
using CoinLib.Wallets;
using CoinLib.Wallets.DataAccess;

namespace CoinLib.Tests.Fixtures
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

		public IWalletRepository Value { get; set; }
	}
}