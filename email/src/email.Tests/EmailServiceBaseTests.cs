using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace email.Tests
{
    [TestFixture]
    public class EmailServiceBaseTests : MessageTests
    {
        [Test]
        public void Can_render_liquid_to_text_body()
        {
            var harness = new InMemoryEmailService();
            var message = harness.CreateTextEmail("Hello, {{ thing }}!", new { thing = "World"});
            harness.Send(message);
            Assert.AreEqual(1, harness.Messages.Count);
            Assert.AreEqual("Hello, World!", harness.Messages.Single().TextBody);
        }

        [Test]
        public void Can_render_liquid_to_html_body()
        {
            var harness = new InMemoryEmailService();
            var message = harness.CreateHtmlEmail("<html><body>Hello, {{ thing }}!<html></body>", new { thing = "World" });
            harness.Send(message);
            Assert.AreEqual(1, harness.Messages.Count);
            Assert.AreEqual("<html><body>Hello, World!<html></body>", harness.Messages.Single().HtmlBody);
        }

        [Test]
        public void Can_render_liquid_to_text_and_html_body()
        {
            var harness = new InMemoryEmailService();
            var message = harness.CreateCombinedEmail("<html><body>Hello, {{ thing }}!<html></body>", "Hello, {{ thing }}!", new { thing = "World" });
            harness.Send(message);
            Assert.AreEqual(1, harness.Messages.Count);
            Assert.AreEqual("<html><body>Hello, World!<html></body>", harness.Messages.Single().HtmlBody);
            Assert.AreEqual("Hello, World!", harness.Messages.Single().TextBody);
        }

        [Test]
        public void Can_prepare_templated_messages_at_reasonable_performance()
        {
            const int trials = 10000;
            var harness = new InMemoryEmailService();

            // "Warm-up" cancels first-time cost
            harness.CreateCombinedEmail("<html><body>Hello, {{ thing }}!<html></body>", "Hello, {{ thing }}!", new { thing = "World" });

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < trials; i++)
            {
                var message = harness.CreateCombinedEmail("<html><body>Hello, {{ thing }}!<html></body>", "Hello, {{ thing }}!", new { thing = "World" });
                harness.Send(message);
            }
            sw.Stop();
            Trace.WriteLine(sw.Elapsed);
            Assert.AreEqual(trials, harness.Messages.Count);
        }
    }
}
