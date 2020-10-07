using System.Threading.Tasks;
using ChainLib.Models;
using Xunit;

namespace CoinLib.Tests.Controllers
{
    public class WithBlockchainController : IClassFixture<BlockchainTestFixture>
    {
        private readonly BlockchainTestFixture _fixture;

        public WithBlockchainController(BlockchainTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Can_get_latest_block()
        {
            var block = await _fixture.Client.GetAsync<Block>("/blockchain/blocks/latest");

            Assert.NotNull(block);
        }
    }
}
