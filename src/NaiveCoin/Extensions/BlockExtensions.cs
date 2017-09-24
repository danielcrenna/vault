using NaiveCoin.Core.Providers;
using NaiveCoin.Models;

namespace NaiveCoin.Extensions
{
    public static class BlockExtensions
    {
        public static string ToHash(this Block block, IHashProvider hashProvider)
        {
            return hashProvider.ComputeHash($"{block.Index}{block.PreviousHash}{block.Timestamp}{hashProvider.ComputeHash(block.Transactions)}{block.Nonce}");
        }
    }
}