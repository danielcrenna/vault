using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
{
    public class WhenBlockchainIsCreated : IClassFixture<BlockchainFixture>
    {
        private readonly BlockchainFixture _fixture;

        public WhenBlockchainIsCreated(BlockchainFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void There_are_no_migration_errors() { }
        
    }
}