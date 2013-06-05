using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroMQ;
using copper.ZeroMQ;

namespace copper.Tests.ZeroMQ
{
    [TestFixture]
    public class ZmqProducerTests
    {
        [Test]
        public void Can_send_and_receive_on_messaging_layer()
        {
            var block = new AutoResetEvent(false);

            Task.Factory.StartNew(() =>
            {
                var subscriber = new ZmqSubscriber("tcp://127.0.0.1:5000");
                var next = subscriber.Receive();
                Assert.IsNotNull(next);
                subscriber.Dispose();
                block.Set();
            });

            var publisher = new ZmqPublisher("tcp://127.0.0.1:5000");
            var status = publisher.Send(new byte[1]);
            Assert.AreEqual(SendStatus.Sent, status);

            block.WaitOne();
            publisher.Dispose();
        }

        [Test]
        public void Messages_are_dequeued()
        {
            var block = new AutoResetEvent(false);
            const int sent = 10;
            var received = 0;

            // Anything handled is sent to zmq
            var consumer = new ZmqConsumer<StringEvent>("tcp://127.0.0.1:5000");
            
            // Anything taken from zmq goes to the console and is counted
            var producer = new ZmqProducer<StringEvent>("tcp://127.0.0.1:5000").Consumes(@event =>
            {
                Console.WriteLine(@event.Text);
                received++;
                if (received >= sent)
                {
                    block.Set();
                }
            });
            producer.Start();

            consumer.Handle(new StringEvent("Test1"));
            consumer.Handle(new StringEvent("Test2"));
            consumer.Handle(new StringEvent("Test3"));
            consumer.Handle(new StringEvent("Test4"));
            consumer.Handle(new StringEvent("Test5"));
            consumer.Handle(new StringEvent("Test6"));
            consumer.Handle(new StringEvent("Test7"));
            consumer.Handle(new StringEvent("Test8"));
            consumer.Handle(new StringEvent("Test9"));
            consumer.Handle(new StringEvent("Test10"));
            
            block.WaitOne();
            producer.Dispose();
            consumer.Dispose();
        }
    }
}