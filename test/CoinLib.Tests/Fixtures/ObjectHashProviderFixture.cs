using NaiveChain.Models;

namespace NaiveCoin.Tests.Fixtures
{
    public class ObjectHashProviderFixture
    {
        public ObjectHashProviderFixture()
        {
            Value = new StableHashProvider();
        }

        public StableHashProvider Value { get; set; }
    }
}