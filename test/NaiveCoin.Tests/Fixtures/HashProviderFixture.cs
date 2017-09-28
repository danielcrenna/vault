using NaiveCoin.Core.Providers;

namespace NaiveCoin.Tests.Fixtures
{
    public class HashProviderFixture
    {
        public HashProviderFixture()
        {
            Value = new StableHashProvider();
        }

        public IHashProvider Value { get; set; }
    }
}