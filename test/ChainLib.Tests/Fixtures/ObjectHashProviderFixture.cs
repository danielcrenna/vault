using ChainLib.Models;

namespace ChainLib.Tests.Fixtures
{
    public class ObjectHashProviderFixture
    {
        public ObjectHashProviderFixture()
        {
            Value = new StableHashProvider();
        }

        public IHashProvider Value { get; set; }
    }
}