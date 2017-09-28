namespace NaiveCoin.Wallets
{
    public interface IWalletProvider : 
        IWalletAddressProvider, 
        IWalletSecretProvider,
        IWalletFactoryProvider,
		IWalletRepository
    {
		
    }
}