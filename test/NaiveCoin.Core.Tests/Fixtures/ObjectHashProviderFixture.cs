using NaiveCoin.Core.Providers;

namespace NaiveCoin.Core.Tests.Fixtures
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