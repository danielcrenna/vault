using System.Collections.Generic;
using System.Linq;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Models;

namespace NaiveCoin.Wallets
{
    public class Wallet
    {
        public string Id { get; internal set; }
        public string PasswordHash { get; internal set; }
        public string Secret { get; set; }
        public IList<KeyPair> KeyPairs { get; } = new List<KeyPair>();

        internal Wallet() { /* Required for serialization */ }

        public string GetAddressByIndex(int index)
        {
            return KeyPairs.SingleOrDefault(x => x.Index == index)?.PublicKey;
        }

        public string GetAddressByPublicKey(string publicKey)
        {
            return KeyPairs.SingleOrDefault(x => x.PublicKey == publicKey)?.PublicKey;
        }

        public string GetPrivateKeyByAddress(string publicKey)
        {
            return KeyPairs.SingleOrDefault(x => x.PublicKey == publicKey)?.PrivateKey;
        }

        public IEnumerable<string> GetAddresses()
        {
            return KeyPairs.Select(x => x.PublicKey);
        }

        internal static Wallet FromPassword(string password, string salt = null)
        {
            var wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = CryptoUtil.PasswordHash(password, salt)
            };
            return wallet;
        }

        internal static Wallet FromPasswordHash(string passwordHash)
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