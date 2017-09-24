using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;

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

        public static byte[] FromHex(this string input)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(input));
            Contract.Assert(input.Length % 2 == 0);

            var result = new byte[input.Length / 2];
            for (var i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            return result;
        }

        public static byte[] Sha256(this byte[] input)
        {
            using (var algorithm = SHA256.Create())
                return algorithm.ComputeHash(input);
        }

        public static byte[] Sha256(this string input)
        {
            return Sha256(Encoding.UTF8.GetBytes(input));
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
    }
}