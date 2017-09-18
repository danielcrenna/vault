using System.Collections.Generic;
using System.Linq;
using NaiveCoin.Helpers;

namespace NaiveCoin.Models
{
    public class Wallet
    {
        public string Id { get; private set; }
        public string PasswordHash { get; private set; }
        public string Secret { get; private set; }
        public IList<KeyPair> KeyPairs { get; } = new List<KeyPair>();

        public string GenerateAddress()
        {
            // If secret is null, it is a brand new wallet
            if (string.IsNullOrWhiteSpace(Secret))
                GenerateSecret();

            // Generate next seed based on the first secret or a new secret from the last key pair
            var lastKeyPair = KeyPairs.LastOrDefault();
            var seed = lastKeyPair == null ? Secret : CryptoEdDsaUtil.GenerateSecret(lastKeyPair.SecretKey);

            var keyPairRaw = CryptoEdDsaUtil.GenerateKeyPairFromSecret(seed);
            var newKeyPair = new KeyPair (
                KeyPairs.Count + 1,
                keyPairRaw.Item2.ToHex(),
                keyPairRaw.Item1.ToHex()
            );
            KeyPairs.Add(newKeyPair);
            return newKeyPair.PublicKey;
        }

        public string GenerateSecret()
        {
            return Secret = CryptoEdDsaUtil.GenerateSecret(PasswordHash);
        }

        public string GetAddressByIndex(int index)
        {
            return KeyPairs.SingleOrDefault(x => x.Index == index)?.PublicKey;
        }

        public string GetAddressByPublicKey(string publicKey)
        {
            return KeyPairs.SingleOrDefault(x => x.PublicKey == publicKey)?.PublicKey;
        }

        public string GetSecretKeyByAddress(string publicKey)
        {
            return KeyPairs.SingleOrDefault(x => x.PublicKey == publicKey)?.SecretKey;
        }

        public IEnumerable<string> GetAddresses()
        {
            return KeyPairs.Select(x => x.PublicKey);
        }

        public static Wallet CreateFromPassword(string password)
        {
            var wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = CryptoUtil.Hash(password)
            };
            return wallet;
        }

        public static Wallet CreateFromPasswordHash(string passwordHash)
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