using NaiveChain.Models;

namespace NaiveChain.Tests.Fixtures
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