using NaiveCoin.Core.Providers;

namespace NaiveCoin.Core.Tests.Fixtures
{
    public class ObjectHashProviderFixture
    {
        public ObjectHashProviderFixture()
        {
            Value = new StableObjectHashProvider();
        }

        public StableObjectHashProvider Value { get; set; }
    }
}