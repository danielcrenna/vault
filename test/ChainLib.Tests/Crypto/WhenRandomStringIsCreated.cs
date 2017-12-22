using ChainLib.Tests.Crypto.Fixtures;
using Xunit;

namespace ChainLib.Tests.Crypto
{
    public class WhenRandomStringIsCreated : IClassFixture<RandomStringFixture>
    {
        private readonly RandomStringFixture _fixture;

        public WhenRandomStringIsCreated(RandomStringFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void It_is_the_correct_length()
        {
            Assert.Equal(64, _fixture.Value.Length);
        }
    }
}
