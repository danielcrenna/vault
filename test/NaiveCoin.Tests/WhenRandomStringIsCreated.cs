using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
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
