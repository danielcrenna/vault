using System;
using System.Security.Cryptography;
using System.Text;
using NaiveCoin.Models;

namespace NaiveCoin.Helpers
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

        internal static string ToHex(this byte[] value)
        {
            var sb = new StringBuilder();
            foreach (var b in value)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static readonly IObjectHashProvider Provider = new StableObjectHashProvider(SHA256.Create());

        public static string Hash(object any)
        {
            return Provider.ComputeHash(any);
        }

        public static byte[] HashBytes(object any)
        {
            return Provider.ComputeHashBytes(any);
        }
    }
}