namespace NaiveCoin.Wallets
{
    /// <summary>
    /// "Brain wallets" are wallets whose private key is generated through human memory.
    /// These wallets are less secure, because the private key can be obtained by anyone who
    /// memorizes the passphrase, or any machine that guesses the passphrase correctly.
    /// 
    /// They are more convenient for humans, though, since all that is required to gain
    /// access to a wallet address is knowledge of the passphrase, rather than also requiring
    /// a valid copy of the private key.
    /// </summary>
    public class BrainWalletProvider : IWalletProvider
    {
        private readonly PasswordHashSecretProvider _secrets;
        private readonly DeterministicWalletAddressProvider _addresses;
        private readonly FixedSaltWalletFactoryProvider _factory;

        public BrainWalletProvider(string salt = "_NaiveCoin_Salt_")
        {
            _secrets = new PasswordHashSecretProvider();
            _addresses = new DeterministicWalletAddressProvider(_secrets);
            _factory = new FixedSaltWalletFactoryProvider(salt);
        }
        
        public string GenerateAddress(Wallet wallet)
        {
            return _addresses.GenerateAddress(wallet);
        }

        public byte[] GenerateSecret(Wallet wallet)
        {
            return _secrets.GenerateSecret(wallet);
        }

        public Wallet Create(string password)
        {
            return _factory.Create(password);
        }
    }
}