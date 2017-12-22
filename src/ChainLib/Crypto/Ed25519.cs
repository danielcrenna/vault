using System;
using System.Text;
using Sodium;

namespace ChainLib.Crypto
{
    public class Ed25519
    {
	    public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
	    {
		    var keypair = PublicKeyAuth.GenerateKeyPair(privateKeySeed);

            return new Tuple<byte[], byte[]>(keypair.PublicKey, keypair.PrivateKey);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromPrivateKey(byte[] privateKey)
        {
	        var publicKey = PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(privateKey);
			
            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static byte[] Sign(Tuple<byte[], byte[]> keyPair, string message)
        {
	        return PublicKeyAuth.Sign(Encoding.UTF8.GetBytes(message), keyPair.Item2);
        }

        public static bool VerifySignature(byte[] publicKey, byte[] signature, byte[] message)
        {
	        return PublicKeyAuth.VerifyDetached(signature, message, publicKey);
        }
    }
}