using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Linq;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using NUnit.Framework;
using copper.AmazonSQS;

namespace copper.Tests.AmazonSQS
{
    [TestFixture]
    public class SimpleQueueConsumerTests
    {
        [Test]
        public void Messages_are_queued()
        {
            var client = new AmazonSQSClient(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSSecret"]);
            var queue = Guid.NewGuid().ToString();
            var consumer = new SimpleQueueConsumer<StringEvent>(client, queue);
            var block = new AutoResetEvent(false);
            
            var producer = new ObservingProducer<StringEvent>();
            producer.Attach(consumer);
            producer.Produces(GetPayload().ToObservable());
            producer.Start();

            block.WaitOne(TimeSpan.FromSeconds(5));
            client.DeleteQueue(new DeleteQueueRequest().WithQueueUrl(consumer.QueueUrl));
        }

        private static IEnumerable<StringEvent> GetPayload()
        {
            var messages = new ConcurrentQueue<StringEvent>();
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            messages.Enqueue(new StringEvent("Test!"));
            return messages;
        }
    }
}
