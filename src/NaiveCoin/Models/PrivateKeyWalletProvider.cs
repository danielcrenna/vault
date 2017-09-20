namespace NaiveCoin.Models
{
    /// <summary>
    /// Generates a wallet using high-entropy, cryptographically secure random data.
    /// 
    /// This type of wallet is more secure, since the private key must be obtained in addition
    /// to any additional security, and cannot be re-created by any other means.
    /// </summary>
    public class PrivateKeyWalletProvider : IWalletProvider
    {
        public string GenerateAddress(Wallet wallet)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateSecret(Wallet wallet)
        {
            throw new System.NotImplementedException();
        }

        public Wallet CreateFromPassword(string password)
        {
            throw new System.NotImplementedException();
        }

        public Wallet CreateFromPasswordHash(string passwordHash)
        {
            throw new System.NotImplementedException();
        }
    }
}