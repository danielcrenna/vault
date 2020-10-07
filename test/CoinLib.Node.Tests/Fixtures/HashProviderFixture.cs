using ChainLib.Models;

namespace CoinLib.Tests.Fixtures
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