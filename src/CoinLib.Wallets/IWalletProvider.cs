namespace CoinLib.Wallets
{
    public interface IWalletProvider : 
        IWalletAddressProvider, 
        IWalletSecretProvider,
        IWalletFactoryProvider,
		IWalletRepository
    {
		
    }
}