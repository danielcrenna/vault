using NUnit.Framework;
using email.Templates;

namespace email.Postmark.Tests
{
    [TestFixture]
    [Ignore]
    public class PostmarkEmailProviderTests
    {
        private const string POSTMARK_SERVER_TOKEN = "02e7d83b-0843-4b1b-b0ed-65ed932983aa";
        private const string POSTMARK_FROM_ADDRESS = "daniel.crenna@gmail.com";
        private const string POSTMARK_TO_ADDRESS = "daniel.crenna@gmail.com";

        [Test]
        public void Can_send_email_with_postmark_provider()
        {
            var engine = new DotLiquidEmailTemplateEngine();
            var provider = new PostmarkEmailProvider(POSTMARK_SERVER_TOKEN);
            var message = engine.CreateTextEmail("Hello, {{ YouThere }}!", new { YouThere = "Postmark", From = POSTMARK_FROM_ADDRESS, To = POSTMARK_TO_ADDRESS, Subject = "email test" });
            Assert.IsTrue(provider.Send(message));
        }
    }
}
