using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NaiveChain.Models;
using NaiveChain.Tests.Fixtures;
using Xunit;

namespace NaiveChain.Tests
{
	public class WhenBlockDatabaseContainsOnlyGenesis :
		IClassFixture<EmptyBlockchainFixture>,
		IClassFixture<ObjectHashProviderFixture>
	{
		public WhenBlockDatabaseContainsOnlyGenesis(
			EmptyBlockchainFixture blockDatabase, 
			ObjectHashProviderFixture hashProvider)
		{
			Fixture = blockDatabase;
			HashProvider = hashProvider.Value;
		}

		public IHashProvider HashProvider { get; set; }
		public EmptyBlockchainFixture Fixture { get; set; }

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
	}
}