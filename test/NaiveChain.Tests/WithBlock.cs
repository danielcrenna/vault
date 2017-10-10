using System;
using System.Collections.Generic;
using NaiveChain.Extensions;
using NaiveChain.Models;
using NaiveChain.Tests.Fixtures;
using NaiveCoin.Core;
using Xunit;

namespace NaiveChain.Tests
{
	public class WithBlock : IClassFixture<ObjectHashProviderFixture>
	{
		private readonly ObjectHashProviderFixture _hash;

		public WithBlock(ObjectHashProviderFixture hash)
		{
			_hash = hash;
		}

		[Fact]
		public void Empty_object_collection_equivalent_to_null()
		{
			var block = new Block();
			block.Nonce = 1;
			block.PreviousHash = "rosebud".Sha256();
			block.Timestamp = DateTimeOffset.UtcNow.Ticks;
			block.Hash = block.ToHashBytes(_hash.Value);

			block.Objects = new List<BlockObject>();
			Assert.Equal(block.Hash, block.ToHashBytes(_hash.Value));
		}

		[Fact]
		public void Consecutive_hashing_is_idempotent()
		{
			var block = new Block();
			block.Nonce = 1;
			block.PreviousHash = "rosebud".Sha256();
			block.Timestamp = DateTimeOffset.UtcNow.Ticks;
			block.Hash = block.ToHashBytes(_hash.Value);

			Assert.Equal(block.Hash, block.ToHashBytes(_hash.Value));
		}

		[Fact]
		public void Can_round_trip_with_no_objects()
		{
			var block = new Block();
			block.Nonce = 1;
			block.PreviousHash = "rosebud".Sha256();
			block.Timestamp = DateTimeOffset.UtcNow.Ticks;
			block.Hash = block.ToHashBytes(_hash.Value);
			block.RoundTripCheck(_hash.Value);
		}
	}
}