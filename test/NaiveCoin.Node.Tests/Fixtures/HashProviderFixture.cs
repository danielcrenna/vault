using NaiveChain.Models;
using NaiveCoin.Core;

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