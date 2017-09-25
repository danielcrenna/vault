using System;
using System.Security.Cryptography;
using System.Text;
using Sodium;

namespace NaiveCoin.Core.Helpers
{
    public class CryptoEdDsaUtil
    {
        public static byte[] GenerateSecret(string seed)
        {
            return Pbkdf2.CreateRawHash(seed, "salt", 64000, 32, HashAlgorithmName.SHA512);
        }

        [Obsolete]
        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(string secret)
        {
            return GenerateKeyPairFromSecret(Encoding.UTF8.GetBytes(secret));
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
        {
            Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey,
                privateKeySeed);

            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromPrivateKey(byte[] privateKey)
        {
            Ed25519.KeyPairFromPrivateKey(out var publicKey, privateKey);
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