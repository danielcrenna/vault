using NaiveChain.Models;
using NaiveCoin.Core;
using NaiveCoin.Models;

namespace NaiveCoin.Extensions
{
    public static class BlockExtensions
    {
        public static string ToHash(this CurrencyBlock block, IHashProvider hashProvider)
        {
			return hashProvider.ComputeHash($"{block.Index}{block.PreviousHash}{block.Timestamp}{hashProvider.ComputeHash(block.Transactions)}{hashProvider.ComputeHash(block.Objects)}{block.Nonce}");
        }
    }
}