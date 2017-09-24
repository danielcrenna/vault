using System;
using System.Security.Cryptography;
using System.Text;
using Chaos.NaCl;

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
            var privateKeySeed = Encoding.UTF8.GetBytes(secret);

            return GenerateKeyPairFromSecret(privateKeySeed);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
        {
            Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey,
                privateKeySeed);

            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromPrivateKey(byte[] privateKeySeed, byte[] privateKey)
        {
            // We cannot derive a public key directly, so we need to regenerate the wallet keypair,
            // Then use that to regenerate an address for the given privateKey, which is normally used as a 
            // secret, in the normal way. 
            // 
            // NOTE: This assumes this is possible for the wallet implementation.
            // (See: https://bitcoin.stackexchange.com/a/42456)

            /*
                char           hex_ed_pk[65];                                      
                unsigned char  seed[crypto_sign_SEEDBYTES];                       
                unsigned char  ed25519_skpk[crypto_sign_ed25519_SECRETKEYBYTES];        
                unsigned char  ed25519_pk[crypto_sign_ed25519_PUBLICKEYBYTES];

                // COMPUTE ED25519 PUBLIC KEY, REQUIRES ESTABLISHING A SEED
                (void)crypto_sign_ed25519_sk_to_seed( seed,ed25519_skpk);                
                (void)crypto_sign_seed_keypair( ed25519_pk, ed25519_skpk, seed );      
                (void)sodium_bin2hex( hex_ed_pk, 65, ed25519_pk, 32 );
             */

            byte[] expandedPrivateKey = Ed25519.ExpandedPrivateKeyFromSeed(privateKeySeed);
            byte[] seed = Ed25519.Sign(privateKeySeed, expandedPrivateKey);
            Ed25519.KeyPairFromSeed(out var publicKey, out byte[] privateKey2, seed);
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