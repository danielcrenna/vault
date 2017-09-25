using System;
using System.Security.Cryptography;
using System.Text;
using Sodium;

namespace NaiveCoin.Core.Helpers
{
    public class CryptoEdDsaUtil
    {
	    public static byte[] GenerateSecret(byte[] seed)
	    {
		    return GenerateSecret(seed.ToHex());
	    }

		public static byte[] GenerateSecret(string seed)
        {
            return Pbkdf2.CreateRawHash(seed, "salt", 64000, 32, HashAlgorithmName.SHA512);
        }
		
        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
        {
            Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey,
                privateKeySeed);

            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromPrivateKey(byte[] privateKey)
        {
            Ed25519.PublicKeyFromPrivateKey(out var publicKey, privateKey);
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