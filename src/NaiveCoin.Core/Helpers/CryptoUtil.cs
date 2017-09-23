using System;
using System.Security.Cryptography;
using System.Text;
using NaiveCoin.Core.Providers;

namespace NaiveCoin.Core.Helpers
{
    public static class CryptoUtil
    {
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        public static string RandomString(int size = 64)
        {
            var random = new byte[(int)Math.Floor(size / 2m)];
            Random.GetNonZeroBytes(random);
            return ToHex(random);
        }

        public static string ToHex(this byte[] value)
        {
            var sb = new StringBuilder();
            foreach (var b in value)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Produces a password hash suitable for long term storage. This means using a random salt per password, high entropy, and
        /// high number of key stretching iterations.
        /// 
        /// It's important to distinguish this from a Wallet address' private key.
        /// Normally, unless you're creating a "brain wallet", this should never be used as the seed for a private key, since
        /// remembering the password is the only thing necessary to derive a private key.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string PasswordHash(string password, string salt = null)
        {
            return salt != null
                ? Pbkdf2.CreateRawHash(password, salt, 64000, 512, HashAlgorithmName.SHA512).ToHex()
                : Pbkdf2.CreateStorageHash(password, 24, 64000, 512, HashAlgorithmName.SHA512);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            return Pbkdf2.VerifyPassword(password, passwordHash);
        }

        public static bool SlowEquals(byte[] a, byte[] b)
        {
            return Pbkdf2.SlowEquals(a, b);
        }

        private static readonly IObjectHashProvider Provider = new StableObjectHashProvider(SHA256.Create());

        public static string ObjectHash(object any)
        {
            return Provider.ComputeHash(any);
        }

        public static byte[] ObjectHashBytes(object any)
        {
            return Provider.ComputeHashBytes(any);
        }
    }
}