using NaiveChain.Models;

namespace NaiveChain.Extensions
{
	public static class BlockExtensions
	{
		public static string ToHashString(this Block block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashString(block);
		}

		public static byte[] ToHashBytes(this Block block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashBytes(block);
		}
	}
}