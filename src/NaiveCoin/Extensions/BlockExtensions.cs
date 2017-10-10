using NaiveChain.Models;
using NaiveCoin.Models;

namespace NaiveCoin.Extensions
{
    public static class BlockExtensions
    {
        public static string ToHash(this CurrencyBlock block, IHashProvider hashProvider)
        {
	        return hashProvider.ComputeHash(block);
        }

		public static byte[] ToHashBytes(this CurrencyBlock block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashBytes(block);
	    }
	}
}