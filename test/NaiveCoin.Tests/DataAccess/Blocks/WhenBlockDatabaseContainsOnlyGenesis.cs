using Microsoft.Data.Sqlite;
using NaiveCoin.Models;
using NaiveCoin.Tests.Fixtures;
using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;
using Xunit;

namespace NaiveCoin.Tests.DataAccess.Blocks
{
    public class WhenBlockDatabaseContainsOnlyGenesis : 
        IClassFixture<BlockDatabaseWithGenesisBlockFixture>, 
        IClassFixture<CoinSettingsFixture>, 
        IClassFixture<ObjectHashProviderFixture>
    {
        public WhenBlockDatabaseContainsOnlyGenesis(BlockDatabaseWithGenesisBlockFixture blockDatabase, CoinSettingsFixture coinSettings, ObjectHashProviderFixture objectHashProvider)
        {
            Fixture = blockDatabase;
            CoinSettings = coinSettings.Value;
            ObjectHashProvider = objectHashProvider.Value;
        }

        public IObjectHashProvider ObjectHashProvider { get; set; }

        public BlockDatabaseWithGenesisBlockFixture Fixture { get; set; }
        public CoinSettings CoinSettings { get; }

        [Fact]
        public void There_are_no_migration_errors() { }

        [Fact]
        public void Cannot_add_unhashed_block()
        {
            Assert.Throws<SqliteException>(() =>
            {
                Block block = new Block();
                Fixture.Value.Add(block);
            });
        }

        [Fact]
        public void Cannot_add_non_unique_block()
        {
            Assert.Throws<SqliteException>(() =>
            {
                Block block = CoinSettings.GenesisBlock;
                BeforeSave(block);
                Fixture.Value.Add(block);
            });
        }

        [Fact]
        public void Can_retrieve_genesis_block_by_index()
        {
            var retrieved = Fixture.Value.GetByIndex(1);
            Assert.NotNull(retrieved);
        }
        
        [Fact]
        public void Can_retrieve_genesis_block_by_hash()
        {
            Block block = CoinSettings.GenesisBlock;
            BeforeSave(block);

            var retrieved = Fixture.Value.GetByHash(block.Hash);
            Assert.NotNull(retrieved);
        }

        [Fact]
        public void Length_is_one()
        {
            var retrieved = Fixture.Value.GetLength();
            Assert.Equal(1, retrieved);
        }

        [Fact]
        public void Last_block_is_genesis_block()
        {
            var genesis = CoinSettings.GenesisBlock;
            var retrieved = Fixture.Value.GetLastBlock();
            Assert.Equal(genesis.Timestamp, retrieved.Timestamp);
        }

        private void BeforeSave(Block block)
        {
            foreach (var transaction in block.Transactions)
                transaction.Hash = transaction.ToHash(ObjectHashProvider);
            block.Hash = block.ToHash(ObjectHashProvider);
        }
    }
}