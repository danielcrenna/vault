using ChainLib.Crypto;

namespace ChainLib.Wallets.Factories
{
    public class SaltedWalletFactoryProvider : IWalletFactoryProvider
    {
        public Wallet Create(string password)
        {
            return Wallet.FromPassword(password);
        }

	    public Wallet Create(params object[] args)
	    {
		    if (args.Length == 0)
		    {
				return new Wallet
				{
					Id = CryptoUtil.RandomString()
				};
		    }
		    return args.Length != 1 ? null : Create(args[0]?.ToString());
	    }
    }
}