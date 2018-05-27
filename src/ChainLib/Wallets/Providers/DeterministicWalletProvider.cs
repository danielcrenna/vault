using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Wallets.Addresses;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.Secrets;

namespace ChainLib.Wallets.Providers
{
	public class DeterministicWalletProvider : IWalletProvider
	{
		private readonly IWalletRepository _repository;
		private readonly IWalletSecretProvider _secrets;
		private readonly IWalletAddressProvider _addresses;
		private readonly IWalletFactoryProvider _factory;

		public DeterministicWalletProvider(IWalletRepository repository, ushort bitsOfEntropy = 256)
		{
			_repository = repository;
			_secrets = new RandomWalletSecretProvider(bitsOfEntropy);
			_addresses = new DeterministicWalletAddressProvider(_secrets);
			_factory = new SaltedWalletFactoryProvider();
		}

		public string GenerateAddress(Wallet wallet)
		{
			return _addresses.GenerateAddress(wallet);
		}

		public byte[] GenerateSecret(params object[] args)
		{
			return _secrets.GenerateSecret(args);
		}

		public Wallet Create(params object[] args)
		{
			return _factory.Create(args);
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