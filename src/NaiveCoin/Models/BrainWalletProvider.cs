using System.Linq;
using NaiveCoin.Helpers;

namespace NaiveCoin.Models
{
    /// <summary>
    /// "Brain wallets" are wallets whose private key is generated through human memory.
    /// These wallets are less secure, because the private key can be obtained by anyone who
    /// memorizes the passphrase, or any machine that guesses the password correctly.
    /// 
    /// They are more convenient for humans, though, since all that is required to gain
    /// access to a wallet address is knowledge of the passphrase, rather than also requiring
    /// a valid copy of the private key.
    /// </summary>
    public class BrainWalletProvider : IWalletProvider
    {
        public string GenerateAddress(Wallet wallet)
        {
            // If secret is null, it is a brand new wallet
            if (string.IsNullOrWhiteSpace(wallet.Secret))
                GenerateSecret(wallet);

            // Generate next seed based on the first secret or a new secret from the last key pair
            var lastKeyPair = wallet.KeyPairs.LastOrDefault();
            var seed = lastKeyPair == null ? wallet.Secret : CryptoEdDsaUtil.GenerateSecret(lastKeyPair.SecretKey);

            var keyPairRaw = CryptoEdDsaUtil.GenerateKeyPairFromSecret(seed);
            var newKeyPair = new KeyPair(
                wallet.KeyPairs.Count + 1,
                keyPairRaw.Item2.ToHex(),
                keyPairRaw.Item1.ToHex()
            );
            wallet.KeyPairs.Add(newKeyPair);
            return newKeyPair.PublicKey;
        }

        public string GenerateSecret(Wallet wallet)
        {
            wallet.Secret = CryptoEdDsaUtil.GenerateSecret(wallet.PasswordHash);

            return wallet.Secret;
        }

        public Wallet CreateFromPassword(string password)
        {
            var wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = CryptoUtil.Hash(password)
            };
            return wallet;
        }

        public Wallet CreateFromPasswordHash(string passwordHash)
        {
            var wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = passwordHash
            };
            return wallet;
        }
    }
}