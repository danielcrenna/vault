using NUnit.Framework;

namespace copper.Tests
{
    [TestFixture]
    public class EventAggregatorTests
    {
        [Test]
        public void Publishes_to_simple_subscriber()
        {
            var aggregator = new Hub();

            var handled = false;
            aggregator.Subscribe<StringEvent>(se => { handled = true; });

            var sent = aggregator.Publish(new StringEvent("Foo"));
            Assert.IsTrue(sent);
            Assert.IsTrue(handled);
        }

        [Test]
        public void Publishes_to_subscriber_by_topic()
        {
            var aggregator = new Hub();

            var handled = false;
            aggregator.Subscribe<StringEvent>(se => { handled = true; }, @event => @event.Text == "bababooey!");

            var sent = aggregator.Publish(new StringEvent("not bababooey!"));
            Assert.IsTrue(sent);
            Assert.IsFalse(handled);

            sent = aggregator.Publish(new StringEvent("bababooey!"));
            Assert.IsTrue(sent);
            Assert.IsTrue(handled);
        }

        [Test]
        public void Publishes_to_handler()
        {
            var aggregator = new Hub();

            var handler = new StringEventHandler();    
            aggregator.Subscribe(handler);

            var sent = aggregator.Publish(new StringEvent("Foo"));
            Assert.IsTrue(sent);
            Assert.IsTrue(handler.Handled);
        }
    }
}
