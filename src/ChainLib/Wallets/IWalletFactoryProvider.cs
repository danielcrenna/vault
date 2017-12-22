namespace ChainLib.Wallets
{
    public interface IWalletFactoryProvider
    {
        Wallet Create(string password);
    }
}