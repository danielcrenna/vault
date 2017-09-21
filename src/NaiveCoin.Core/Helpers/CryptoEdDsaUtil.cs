using System;
using System.Security.Cryptography;
using System.Text;
using Chaos.NaCl;

namespace NaiveCoin.Core.Helpers
{
    public class CryptoEdDsaUtil
    {
        private const string Salt = "0ffaa74d206930aaece253f090c88dbe6685b9e66ec49ad988d84fd7dff230d1";

        public static string GenerateSecret(string password)
        {
            var hash = Pbkdf2.CreateRawHash(password, Salt, 10000, 512, HashAlgorithmName.SHA512);

            return hash.ToHex();
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(string secret)
        {
            var privateKeySeed = Encoding.UTF8.GetBytes(secret);

            return GenerateKeyPairFromSecret(privateKeySeed);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
        {
            Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey,
                privateKeySeed);

            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static byte[] SignHash(Tuple<byte[], byte[]> keyPair, string message)
        {
            var signature = Ed25519.Sign(Encoding.UTF8.GetBytes(message), keyPair.Item2);

            return signature;
        }

        public static bool VerifySignature(byte[] publicKey, byte[] signature, byte[] message)
        {
            return Ed25519.Verify(signature, message, publicKey);
        }
    }
}