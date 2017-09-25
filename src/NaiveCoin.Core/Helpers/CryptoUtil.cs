using System;
using System.Security.Cryptography;
using System.Text;
using Sodium;

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

        public static string ToHex(this byte[] input)
        {
	        return Utilities.BinaryToHex(input);
        }

        public static byte[] FromHex(this string input)
        {
	        return Utilities.HexToBinary(input);
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

	    public static bool SlowEquals(byte[] a, byte[] b)
	    {
		    return Utilities.Compare(a, b); // WARNING: I am assuming this is constant time!
	    }
	}
}