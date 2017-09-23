namespace NaiveCoin.Wallets
{
    public class SaltedWalletFactoryProvider : IWalletFactoryProvider
    {
        public Wallet Create(string password)
        {
            return Wallet.FromPassword(password);
        }
    }
}