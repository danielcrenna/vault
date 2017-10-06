using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NaiveCoin.Core.Providers;
using NaiveCoin.Extensions;
using NaiveCoin.Models;
using NaiveCoin.Tests.Fixtures;
using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;
using Xunit;

namespace NaiveCoin.Tests.DataAccess.Blocks
{
    public class WhenBlockDatabaseContainsOnlyGenesis : 
        IClassFixture<EmptyBlockchainFixture>, 
        IClassFixture<CoinSettingsFixture>, 
        IClassFixture<HashProviderFixture>
    {
        public WhenBlockDatabaseContainsOnlyGenesis(EmptyBlockchainFixture blockDatabase, CoinSettingsFixture coinSettings, HashProviderFixture hashProvider)
        {
            Fixture = blockDatabase;
            CoinSettings = coinSettings.Value;
            HashProvider = hashProvider.Value;
        }

        public IHashProvider HashProvider { get; set; }
        public EmptyBlockchainFixture Fixture { get; set; }
        public CoinSettings CoinSettings { get; }

        [Fact]
        public void There_are_no_migration_errors() { }

        [Fact]
        public async Task Cannot_add_unhashed_block()
        {
            await Assert.ThrowsAsync<SqliteException>(async () =>
            {
	            CurrencyBlock block = new CurrencyBlock();
                await Fixture.Value.AddAsync(block);
            });
        }

        [Fact]
        public async Task Cannot_add_non_unique_block()
        {
            await Assert.ThrowsAsync<SqliteException>(async () =>
            {
	            CurrencyBlock block = CoinSettings.GenesisBlock;
                BeforeSave(block);
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
	        CurrencyBlock block = CoinSettings.GenesisBlock;
            BeforeSave(block);

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
            var genesis = CoinSettings.GenesisBlock;
            var retrieved = await Fixture.Value.GetLastBlockAsync();
            Assert.Equal(genesis.Timestamp, retrieved.Timestamp);
        }

        private void BeforeSave(CurrencyBlock block)
        {
            foreach (var transaction in block.Transactions)
                transaction.Hash = transaction.ToHash(HashProvider);
            block.Hash = block.ToHash(HashProvider);
        }
    }
}