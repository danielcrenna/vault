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

	    public static byte[] RandomBytes(int size)
	    {
		    return SodiumCore.GetRandomBytes(size);

			var random = new byte[size];
		    Random.GetNonZeroBytes(random);
		    return random;
	    }

		public static string ToHex(this byte[] input)
        {
			// https://stackoverflow.com/a/3974535
	        char[] c = new char[input.Length * 2];

	        byte b;

	        for (int bx = 0, cx = 0; bx < input.Length; ++bx, ++cx)
	        {
		        b = ((byte)(input[bx] >> 4));
		        c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

		        b = ((byte)(input[bx] & 0x0F));
		        c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
	        }

	        return new string(c);

			return Utilities.BinaryToHex(input);
        }

        public static byte[] FromHex(this string input)
        {
			// https://stackoverflow.com/a/3974535
	        if (input.Length == 0 || input.Length % 2 != 0)
		        return new byte[0];

	        byte[] buffer = new byte[input.Length / 2];
	        char c;
	        for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
	        {
		        // Convert first half of byte
		        c = input[sx];
		        buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

		        // Convert second half of byte
		        c = input[++sx];
		        buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
	        }

	        return buffer;

			return Utilities.HexToBinary(input);
        }

        public static byte[] Sha256(this byte[] input)
        {
	        return CryptoHash.Sha256(input);

            using (var algorithm = SHA256.Create())
                return algorithm.ComputeHash(input);
        }

	    public static byte[] Sha512(this byte[] input)
	    {
		    return CryptoHash.Sha512(input);

		    using (var algorithm = SHA512.Create())
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