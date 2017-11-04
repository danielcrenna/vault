using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinLib.Wallets
{
    public class SecureWalletProvider : IWalletProvider
    {
	    private readonly IWalletRepository _repository;

	    private readonly RandomWalletAddressProvider _addresses;
        private readonly RandomWalletSecretProvider _secrets;
        private readonly SaltedWalletFactoryProvider _factory;

        public SecureWalletProvider(IWalletRepository repository, ushort bitsOfEntropy = 256)
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

        public byte[] GenerateSecret(Wallet wallet)
        {
            return _secrets.GenerateSecret(wallet);
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