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
        public byte[] Secret { get; set; }
        public IList<KeyPair> KeyPairs { get; } = new List<KeyPair>();

        internal Wallet() { /* Required for serialization */ }

        public byte[] GetAddressByIndex(int index)
        {
            return KeyPairs.SingleOrDefault(x => x.Index == index)?.PublicKey;
        }

        public byte[] GetAddressByPublicKey(byte[] publicKey)
        {
            return KeyPairs.SingleOrDefault(x => CryptoUtil.SlowEquals(x.PublicKey, publicKey))?.PublicKey;
        }

        public byte[] GetPrivateKeyByAddress(byte[] publicKey)
        {
            return KeyPairs.SingleOrDefault(x => CryptoUtil.SlowEquals(x.PublicKey, publicKey))?.PrivateKey;
        }

        public IEnumerable<byte[]> GetAddresses()
        {
            return KeyPairs.Select(x => x.PublicKey);
        }

        internal static Wallet FromPassword(string password, string salt = null)
        {
            var wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = PasswordUtil.StorageHash(password, salt)
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