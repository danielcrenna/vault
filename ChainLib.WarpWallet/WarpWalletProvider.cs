using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Wallets;

namespace ChainLib.WarpWallet
{
	/// <summary>
	/// Uses warpwallet's algorithm to produce a wallet secret:
	/// <code>
	///     s1 = scrypt(key=(passphrase||0x1), salt=(salt||0x1), N=2^18, r=8, p=1, dkLen=32)
	///     s2 = pbkdf2(key=(passphrase||0x2), salt=(salt||0x2), c=2^16, dkLen=32, prf=HMAC_SHA256)
	/// </code>
	/// <see href="https://keybase.io/warp" />
	/// </summary>
	public class WarpWalletProvider : IWalletProvider
	{
		private readonly IWalletRepository _repository;
		private readonly IWalletSecretProvider _secrets;
		private readonly IWalletAddressProvider _addresses;
		private readonly IWalletFactoryProvider _factory;

		public WarpWalletProvider(IWalletRepository repository, IWalletAddressProvider addresses, IWalletFactoryProvider factory)
		{
			_repository = repository;
			_secrets = new WarpWalletSecretProvider();
			_addresses = addresses;
			_factory = factory;
		}

		public string GenerateAddress(Wallet wallet)
		{
			return _addresses.GenerateAddress(wallet);
		}

		public byte[] GenerateSecret(params object[] args)
		{
			return _secrets.GenerateSecret(args);
		}

		public Wallet Create(string password)
		{
			return _factory.Create(password);
		}

		public Task<IEnumerable<Wallet>> GetAllAsync()
		{
			return _repository.GetAllAsync();
		}

		public Task<Wallet> GetByIdAsync(string id)
		{
			return _repository.GetByIdAsync(id);
		}

		public Task<Wallet> AddAsync(Wallet wallet)
		{
			return _repository.AddAsync(wallet);
		}

		public Task SaveAddressesAsync(Wallet wallet)
		{
			return _repository.SaveAddressesAsync(wallet);
		}
	}
}