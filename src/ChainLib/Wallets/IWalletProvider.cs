namespace ChainLib.Wallets
{
    public interface IWalletProvider : 
        IWalletAddressProvider, 
        IWalletSecretProvider,
        IWalletFactoryProvider,
		IWalletRepository
    {
		
    }
}