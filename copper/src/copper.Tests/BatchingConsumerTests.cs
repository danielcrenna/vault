using System;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace copper.Tests
{
    [TestFixture]
    public class BatchingConsumerTests
    {
        [Test]
        public void Receives_payload_in_batches_by_size()
        {
            var block = new AutoResetEvent(false);
            var producer = new ObservingProducer<int>();
            var consumer = new DelegatingBatchingConsumer<int>(ints => Assert.AreEqual(1000, ints.Count));
            
            producer.Attach(consumer);
            producer.Produces(Observable.Range(1, 10000), onCompleted: () => block.Set());
            producer.Start();
            block.WaitOne();
        }

        [Test]
        public void Receives_payload_in_batches_by_interval()
        {
            var block = new AutoResetEvent(false);
            var producer = new ObservingProducer<int>();
            var consumer = new DelegatingBatchingConsumer<int>(ints =>
            {
                Console.WriteLine("{0} in one second.", ints.Count);
                block.Set();
            }, TimeSpan.FromSeconds(1));

            producer.Attach(consumer);
            producer.Produces(Observable.Range(1, 1000000));
            producer.Start();
            block.WaitOne();
        }

        [Test]
        public void Receives_payload_in_batches_by_size_or_interval_with_payload_smaller_than_size()
        {
            var block = new AutoResetEvent(false);
            var producer = new ObservingProducer<int>();
            var consumer = new DelegatingBatchingConsumer<int>(ints =>
            {
                Assert.AreEqual(500, ints.Count);
                block.Set();
            }, 1000, TimeSpan.FromSeconds(3));

            producer.Attach(consumer);
            producer.Produces(Observable.Range(1, 500));
            producer.Start();
            block.WaitOne();
        }
    }
}