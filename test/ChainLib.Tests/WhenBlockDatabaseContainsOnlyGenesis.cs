using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Tests.Fixtures;
using Xunit;

namespace ChainLib.Tests
{
	public class WhenBlockDatabaseContainsOnlyGenesis :
		IClassFixture<EmptyBlockRepositoryFixture>,
		IClassFixture<ObjectHashProviderFixture>
	{
		public WhenBlockDatabaseContainsOnlyGenesis(EmptyBlockRepositoryFixture blockDatabase)
		{
			Fixture = blockDatabase;
			TypeProvider = Fixture.TypeProvider;
			TypeProvider.TryAdd(0, typeof(Transaction));
		}

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
		public async Task Can_retrieve_genesis_block_by_index()
		{
			Block retrieved = await Fixture.Value.GetByIndexAsync(1);
			Assert.NotNull(retrieved);
			Assert.Equal(retrieved.Hash, Fixture.GenesisBlock.Hash);
			Assert.Equal(retrieved.PreviousHash, Fixture.GenesisBlock.PreviousHash);
			Assert.Equal(retrieved.MerkleRootHash, Fixture.GenesisBlock.MerkleRootHash);
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
	}
}