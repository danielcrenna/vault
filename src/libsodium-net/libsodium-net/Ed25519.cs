using System;
using Sodium.Interop;

namespace Sodium
{
	public static class Ed25519
	{
		public static readonly int PublicKeySizeInBytes = 32;
		public static readonly int SignatureSizeInBytes = 64;
		public static readonly int ExpandedPrivateKeySizeInBytes = 32 * 2;
		public static readonly int PrivateKeySeedSizeInBytes = 32; // 64 in Chaos.NaCl
		public static readonly int SharedKeySizeInBytes = 32;

		public static bool Verify(byte[] signature, byte[] message, byte[] publicKey)
		{
			if (signature == null)
				throw new ArgumentNullException(nameof(signature));
			if (message == null)
				throw new ArgumentNullException(nameof(message));
			if (publicKey == null)
				throw new ArgumentNullException(nameof(publicKey));
			if (signature.Length != SignatureSizeInBytes)
				throw new ArgumentException(string.Format("Signature size must be {0}", SignatureSizeInBytes),
					nameof(signature.Length));
			if (publicKey.Length != PublicKeySizeInBytes)
				throw new ArgumentException(string.Format("Public key size must be {0}", PublicKeySizeInBytes),
					nameof(publicKey.Length));

			long bufferLength = SignatureSizeInBytes;
			return SodiumLibrary.crypto_sign_open(signature, ref bufferLength, message, message.Length, publicKey) != 0;
		}

		public static byte[] Sign(byte[] message, byte[] expandedPrivateKey)
		{
			var signature = new byte[SignatureSizeInBytes];
			long bufferLength = SignatureSizeInBytes;
			SodiumLibrary.crypto_sign(signature, ref bufferLength, message, message.Length, expandedPrivateKey);
			return signature;
		}

		public static byte[] PublicKeyFromSeed(byte[] privateKeySeed)
		{
			KeyPairFromSeed(out var publicKey, out var _, privateKeySeed);
			return publicKey;
		}

		public static byte[] ExpandedPrivateKeyFromSeed(byte[] privateKeySeed)
		{
			KeyPairFromSeed(out var _, out var privateKey, privateKeySeed);
			return privateKey;
		}

		public static void KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, byte[] privateKeySeed)
		{
			if (privateKeySeed == null)
				throw new ArgumentNullException(nameof(privateKeySeed));
			if (privateKeySeed.Length != PrivateKeySeedSizeInBytes)
				throw new ArgumentException(nameof(privateKeySeed));

			var pk = new byte[32];
			var sk = new byte[32];

			SodiumLibrary.crypto_sign_seed_keypair(pk, sk, privateKeySeed);

			publicKey = pk;
			expandedPrivateKey = sk;
		}

		// seealso: https://bitcoin.stackexchange.com/a/42456
		public static void PublicKeyFromPrivateKey(out byte[] publicKey, byte[] privateKey)
		{
			var pk = new byte[32]; // alloc
			var sk = new byte[32]; // alloc
			SodiumLibrary.crypto_sign_seed_keypair(pk, sk, privateKey);
			publicKey = pk;
		}
	}
}