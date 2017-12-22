using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Wallets.Addresses;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.Secrets;

namespace ChainLib.Wallets.Providers
{
	/// <summary>
    /// "Brain wallets" are wallets whose private key is generated through human memory.
    /// These wallets are not secure, because the private key can be obtained by anyone who
    /// memorizes the passphrase, or any machine that guesses the passphrase correctly.
    /// 
    /// They are more convenient for humans, though, since all that is required to gain
    /// access to a wallet address is knowledge of the passphrase, rather than also requiring
    /// a valid copy of the private key.
    /// </summary>
    public class BrainWalletProvider : IWalletProvider
    {
	    private readonly IWalletRepository _repository;
		private readonly IWalletSecretProvider _secrets;
        private readonly IWalletAddressProvider _addresses;
        private readonly IWalletFactoryProvider _factory;

        public BrainWalletProvider(IWalletRepository repository, string salt = Constants.DefaultFixedSalt)
        {
	        _repository = repository;
	        _secrets = new PasswordHashSecretProvider();
            _addresses = new DeterministicWalletAddressProvider(_secrets);
            _factory = new FixedSaltWalletFactoryProvider(salt);
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