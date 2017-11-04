using System.Diagnostics.Contracts;

namespace CoinLib.Wallets
{
    public class FixedSaltWalletFactoryProvider : IWalletFactoryProvider
    {
        private readonly string _salt;

        public FixedSaltWalletFactoryProvider(string salt)
        {
	        Contract.Assert(!string.IsNullOrWhiteSpace(salt));
			Contract.Assert(salt.Length == 16);
			_salt = salt;
        }

        public Wallet Create(string password)
        {
			Contract.Assert(!string.IsNullOrWhiteSpace(password));
			return Wallet.FromPassword(password, _salt);
        }
    }
}