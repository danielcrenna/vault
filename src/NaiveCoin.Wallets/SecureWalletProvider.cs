namespace NaiveCoin.Wallets
{
    public class SecureWalletProvider : IWalletProvider
    {
        private readonly RandomWalletAddressProvider _addresses;
        private readonly RandomWalletSecretProvider _secrets;
        private readonly SaltedWalletFactoryProvider _factory;

        public SecureWalletProvider(ushort bitsOfEntropy = 256)
        {
            _addresses = new RandomWalletAddressProvider(bitsOfEntropy);
            _secrets = new RandomWalletSecretProvider(bitsOfEntropy);
            _factory = new SaltedWalletFactoryProvider();
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