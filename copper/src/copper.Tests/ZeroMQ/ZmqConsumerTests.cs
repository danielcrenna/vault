using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using copper.ProtocolBuffers;
using copper.ZeroMQ;

namespace copper.Tests.ZeroMQ
{
    [TestFixture]
    public class ZmqConsumerTests
    {
        [Test]
        public void Messages_are_queued()
        {
            var block = new AutoResetEvent(false);

            var serializer = new ProtocolBuffersSerializer();
            var socket = new ZmqConsumer<StringEvent>("tcp://127.0.0.1:5000", serializer);
            var consumer = new DelegatingConsumer<StringEvent>(Console.WriteLine, socket); // logs to console before forwarding
            var producer = new ObservingProducer<StringEvent>();

            producer.Attach(consumer);
            producer.Produces(GetPayload(), onCompleted: () => block.Set());
            producer.Start();

            block.WaitOne();
            socket.Dispose();
        }

        private static IEnumerable<StringEvent> GetPayload()
        {
            var messages = new ConcurrentQueue<StringEvent>();
            messages.Enqueue(new StringEvent("Test1"));
            messages.Enqueue(new StringEvent("Test2"));
            messages.Enqueue(new StringEvent("Test3"));
            messages.Enqueue(new StringEvent("Test4"));
            messages.Enqueue(new StringEvent("Test5"));
            messages.Enqueue(new StringEvent("Test6"));
            messages.Enqueue(new StringEvent("Test7"));
            messages.Enqueue(new StringEvent("Test8"));
            messages.Enqueue(new StringEvent("Test9"));
            messages.Enqueue(new StringEvent("Test10"));
            return messages;
        }
    }
}
