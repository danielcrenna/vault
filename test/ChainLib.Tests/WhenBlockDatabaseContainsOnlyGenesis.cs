using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChainLib.Crypto;
using Microsoft.Data.Sqlite;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Streaming;
using ChainLib.Tests.Fixtures;
using Xunit;

namespace ChainLib.Tests
{
	public class WhenBlockDatabaseContainsOnlyGenesis :
		IClassFixture<EmptyBlockRepositoryFixture>,
		IClassFixture<ObjectHashProviderFixture>
	{
		public WhenBlockDatabaseContainsOnlyGenesis(
			EmptyBlockRepositoryFixture blockDatabase, 
			ObjectHashProviderFixture hashProvider)
		{
			Fixture = blockDatabase;
			TypeProvider = Fixture.TypeProvider;
			HashProvider = hashProvider.Value;

			TypeProvider.TryAdd(0, typeof(Transaction));
		}

		public IHashProvider HashProvider { get; set; }
		public EmptyBlockRepositoryFixture Fixture { get; set; }
		public IBlockObjectTypeProvider TypeProvider { get; }

		[Fact]
		public void There_are_no_migration_errors() { }

		[Fact]
		public async Task Cannot_add_unhashed_block()
		{
			await Assert.ThrowsAsync<SqliteException>(async () =>
			{
				Block block = new Block();
				await Fixture.Value.AddAsync(block);
			});
		}

		[Fact]
		public async Task Cannot_add_non_unique_block()
		{
			await Assert.ThrowsAsync<SqliteException>(async () =>
			{
				Block block = Fixture.GenesisBlock;
				await Fixture.Value.AddAsync(block);
			});
		}

		[Fact]
		public void Can_retrieve_genesis_block_by_index()
		{
			var retrieved = Fixture.Value.GetByIndexAsync(1);
			Assert.NotNull(retrieved);
		}

		[Fact]
		public void Can_retrieve_genesis_block_by_hash()
		{
			Block block = Fixture.GenesisBlock;
			var retrieved = Fixture.Value.GetByHashAsync(block.Hash);
			Assert.NotNull(retrieved);
		}

		[Fact]
		public async Task Length_is_one()
		{
			var retrieved = await Fixture.Value.GetLengthAsync();
			Assert.Equal(1, retrieved);
		}

		[Fact]
		public async Task Last_block_is_genesis_block()
		{
			var genesis = Fixture.GenesisBlock;
			var retrieved = await Fixture.Value.GetLastBlockAsync();
			Assert.Equal(genesis.Timestamp, retrieved.Timestamp);
		}
		
		[Fact]
		public async Task Can_stream_typed_objects()
		{
			await Fixture.Value.AddAsync(CreateBlock());

			var projection = new BlockObjectProjection(Fixture.Value, TypeProvider);
			var transactions = projection.Stream<Transaction>();
			
			Assert.NotNull(transactions);
			Assert.Equal(1, transactions.Count());
		}

		private Block CreateBlock()
		{
			var transaction = new Transaction
			{
				Id = $"{Guid.NewGuid()}"
			};
			var blockObject = new BlockObject
			{
				Data = transaction,
				Hash = null,
			};
			var block = new Block
			{
				Nonce = 1,
				PreviousHash = "rosebud".Sha256(),
				Timestamp = DateTimeOffset.UtcNow.Ticks,
				Objects = new List<BlockObject> {blockObject}
			};
			block.Hash = block.ToHashBytes(HashProvider);
			return block;
		}
	}
}