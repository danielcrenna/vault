using System.Text;
using Sodium;

namespace ChainLib.Crypto
{
	public static class CryptoUtil
    {
        public static string RandomString(int size = 64)
        {
            return RandomBytes(size / 2).ToHex();
        }

	    public static byte[] RandomBytes(int size)
	    {
		    return SodiumCore.GetRandomBytes(size);
	    }

	    public static string ToHex(this byte[] input)
        {
			return Utilities.BinaryToHex(input);
        }

        public static byte[] FromHex(this string input)
        {
			return Utilities.HexToBinary(input);
        }

	    public static byte[] Sha256(this string input)
	    {
		    return Encoding.UTF8.GetBytes(input).Sha256();
	    }

		public static byte[] Sha256(this byte[] input)
        {
	        return CryptoHash.Sha256(input);
        }
		
	    public static bool ConstantTimeEquals(this byte[] a, byte[] b)
	    {
		    return Utilities.Compare(a, b);
	    }
	}
}