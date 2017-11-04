namespace CoinLib.Wallets
{
    public interface IWalletFactoryProvider
    {
        Wallet Create(string password);
    }
}