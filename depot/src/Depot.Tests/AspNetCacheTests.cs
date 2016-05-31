using NUnit.Framework;
using depot.AspNet;

namespace depot.Tests
{
    [TestFixture]
    public class AspNetCacheTests : CacheTests
    {
        [SetUp]
        public void SetUp()
        {
            Cache = new AspNetCache();
            FileCacheDependency = new AspNetFileCacheDependency();
        }
    }
}
