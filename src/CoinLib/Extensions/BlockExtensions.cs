using ChainLib.Models;
using CoinLib.Models;

namespace CoinLib.Extensions
{
    public static class BlockExtensions
    {
        public static string ToHash(this CurrencyBlock block, IHashProvider hashProvider)
        {
	        return hashProvider.ComputeHashString(block);
        }

		public static byte[] ToHashBytes(this CurrencyBlock block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashBytes(block);
	    }
	}
}