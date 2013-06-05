using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using NUnit.Framework;
using copper.AmazonSQS;
using copper.Json;

namespace copper.Tests.AmazonSQS
{
    [TestFixture]
    public class SimpleQueueProducerTests
    {
        [Test]
        public void Messages_are_dequeued()
        {
            var block = new AutoResetEvent(false);
            var sent = 10;
            var received = 0;

            var serializer = new JsonSerializer();
            var key = ConfigurationManager.AppSettings["AWSKey"];
            var secret = ConfigurationManager.AppSettings["AWSSecret"];
            var queueName = Guid.NewGuid().ToString();

            var client = new AmazonSQSClient(key, secret);

            // Anything put into the queue will go to Amazon SQS
            var queue = new ConcurrentQueue<StringEvent>();
            var consumer = new SimpleQueueConsumer<StringEvent>(client, queueName);
            new CollectionProducer<StringEvent>(queue).Consumes(consumer).Start();

            // Anything taken from Amazon SQS goes to the console
            new SimpleQueueProducer<StringEvent>(client, queueName, serializer).Consumes(new DelegatingConsumer<StringEvent>(@event =>
            {
                Console.WriteLine(@event.Text);
                received++;
                if (received >= sent)
                {
                    block.Set();
                }
            })).Start();

            // Make some messages
            queue.Enqueue(new StringEvent("Test1"));
            queue.Enqueue(new StringEvent("Test2"));
            queue.Enqueue(new StringEvent("Test3"));
            queue.Enqueue(new StringEvent("Test4"));
            queue.Enqueue(new StringEvent("Test5"));
            queue.Enqueue(new StringEvent("Test6"));
            queue.Enqueue(new StringEvent("Test7"));
            queue.Enqueue(new StringEvent("Test8"));
            queue.Enqueue(new StringEvent("Test9"));
            queue.Enqueue(new StringEvent("Test10"));

            block.WaitOne();

            client.DeleteQueue(new DeleteQueueRequest().WithQueueUrl(consumer.QueueUrl));
        }
    }
}