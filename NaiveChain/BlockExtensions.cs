﻿using NaiveCoin.Core.Providers;

namespace NaiveChain
{
	public static class BlockExtensions
	{
		public static string ToHash(this Block block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHash(
				$"{block.Index}{block.PreviousHash}{block.Timestamp}{hashProvider.ComputeHash(block.Objects)}{block.Nonce}");
		}
	}
}