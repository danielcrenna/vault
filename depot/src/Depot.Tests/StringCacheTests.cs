using System.Text;
using NUnit.Framework;

namespace depot.Tests
{
    [TestFixture]
    public class StringCacheTests
    {
        [Test]
        public void Adds_and_retrieves_short_content_safely()
        {
            const string input = "This will not compress";
            var cache = new StringCache();
            var added = cache.Add("short", input);
            Assert.IsTrue(added);
            var retrieved = cache.Get("short");
            Assert.AreEqual(input, retrieved);
        }

        [Test]
        public void Adds_and_retrieves_long_content_safely()
        {
            var sb = new StringBuilder();
            for(var i = 0; i < 1000; i++)
            {
                sb.Append("I will not write loops in unit tests.");
            }
            var input = sb.ToString();

            var cache = new StringCache();
            var added = cache.Add("long", input);
            Assert.IsTrue(added);
            var retrieved = cache.Get("long");
            Assert.AreEqual(input, retrieved);
        }
    }
}