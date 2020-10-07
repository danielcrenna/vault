using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Wallets.Addresses;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.Secrets;

namespace ChainLib.Wallets.Providers
{
	public class RandomWalletProvider : IWalletProvider
    {
	    private readonly IWalletRepository _repository;

	    private readonly RandomWalletAddressProvider _addresses;
        private readonly RandomWalletSecretProvider _secrets;
        private readonly SaltedWalletFactoryProvider _factory;

        public RandomWalletProvider(IWalletRepository repository, ushort bitsOfEntropy = 256)
        {
	        _repository = repository;
			_addresses = new RandomWalletAddressProvider(bitsOfEntropy);
            _secrets = new RandomWalletSecretProvider(bitsOfEntropy);
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