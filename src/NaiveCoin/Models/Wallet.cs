using System.Collections.Generic;
using System.Linq;
using NaiveCoin.Helpers;

namespace NaiveCoin.Models
{
    public class Wallet
    {
        public string Id { get; internal set; }
        public string PasswordHash { get; internal set; }
        public string Secret { get; set; }
        public IList<KeyPair> KeyPairs { get; } = new List<KeyPair>();

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
    }
}