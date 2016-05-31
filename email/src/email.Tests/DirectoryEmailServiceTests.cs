using NUnit.Framework;

namespace email.Tests
{
    [TestFixture]
    public class DirectoryEmailServiceTests : MessageTests
    {
        [Test]
        public void Can_deliver_email_with_text_to_directory()
        {
            var message = MessageFactory.EmailWithText();
            AssertDelivery(message);
        }

        [Test]
        public void Can_deliver_email_with_html_to_directory()
        {
            var message = MessageFactory.EmailWithHtml();
            AssertDelivery(message);
        }

        [Test]
        public void Can_deliver_email_with_text_and_html_to_directory()
        {
            var message = MessageFactory.EmailWithHtmlAndText();
            AssertDelivery(message);
        }
    }
}
