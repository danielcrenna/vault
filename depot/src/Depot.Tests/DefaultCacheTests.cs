using NUnit.Framework;

namespace depot.Tests
{
    [TestFixture]
    public class DefaultCacheTests : CacheTests
    {
        [SetUp]
        public void SetUp()
        {
            Cache = new DefaultCache();
            FileCacheDependency = new DefaultFileCacheDependency();
        }
    }
}