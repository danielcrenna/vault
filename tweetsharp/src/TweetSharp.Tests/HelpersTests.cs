using NUnit.Framework;

namespace TweetSharp.Next.Tests
{
    [TestFixture]
    public class HelpersTests
    {
        [Test]
        public void Can_underscore_camel_cased_words()
        {
            const string e1 = "since_id";
            const string e2 = "in_reply_to_status_id";
            const string e3 = "page";

            Assert.AreEqual(e1, StringHelpers.Underscore("sinceId"));
            Assert.AreEqual(e2, StringHelpers.Underscore("inReplyToStatusId"));
            Assert.AreEqual(e3, StringHelpers.Underscore("page"));
        }
    }
}
