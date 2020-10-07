namespace ChainLib.Wallets
{
    public interface IWalletFactoryProvider
    {
        Wallet Create(params object[] args);
    }
}