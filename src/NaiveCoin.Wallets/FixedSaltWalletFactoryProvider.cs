namespace NaiveCoin.Wallets
{
    public class FixedSaltWalletFactoryProvider : IWalletFactoryProvider
    {
        private readonly string _salt;

        public FixedSaltWalletFactoryProvider(string salt)
        {
            _salt = salt;
        }

        public Wallet Create(string password)
        {
            return Wallet.FromPassword(password, _salt);
        }
    }
}