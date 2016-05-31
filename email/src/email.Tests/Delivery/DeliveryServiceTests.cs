using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using email.Delivery;
using email.Providers;

namespace email.Tests.Delivery
{
    [TestFixture]
    public class DeliveryServiceTests : MessageTests
    {
        [Test]
        public void Can_deliver_single_batch_of_messages_fully()
        {
            const int count = 1000;

            var pickupDirectory = Path.Combine(Path.GetTempPath(), "pickup");
            CreateOrCleanDirectory(pickupDirectory);

            var config = new DeliveryConfiguration {BacklogFolder = "backlog", MaxDegreeOfParallelism = 10};

            var service = new DeliveryService(new DirectoryEmailProvider(pickupDirectory), config);
            var messages = MessageFactory.EmailWithHtmlAndText(count);

            service.Start();
            service.Send(messages);
            service.Stop(DeliveryCancellationHandling.EmptyQueue);
            
            AssertFileCount(pickupDirectory, count);
            CleanDirectory(pickupDirectory);
        }

        [Test]
        public void Can_deliver_single_batch_of_messages_with_overrun_in_backlog()
        {
            const int count = 10000;

            var config = new DeliveryConfiguration {BacklogFolder = "backlog", MaxDegreeOfParallelism = 1};

            CreateOrCleanDirectory(config.BacklogFolder);

            var harness = new InMemoryEmailService();
            var service = new DeliveryService(harness, config);
            var messages = MessageFactory.EmailWithHtmlAndText(count);
            
            service.Start();
            service.Send(messages);
            Thread.Sleep(10);
            service.Stop(DeliveryCancellationHandling.SendToBacklog);

            var backlogged = Directory.GetFiles(config.BacklogFolder).Length;
            var delivered = harness.Messages.Count;
            Assert.AreEqual(count, backlogged + delivered);
            Trace.WriteLine(string.Format("Backlogged: {0}", backlogged));
            Trace.WriteLine(string.Format("Delivered: {0}", delivered));

            CleanDirectory(config.BacklogFolder);
        }

        [Test]
        public void Can_start_and_seed_with_backlog()
        {
            const int backlog = 500;

            var config = new DeliveryConfiguration {BacklogFolder = "backlog", MaxDegreeOfParallelism = 1};

            CreateOrCleanDirectory(config.BacklogFolder);

            var harness = new InMemoryEmailService();
            var service = new DeliveryService(harness, config);
            var messages = MessageFactory.EmailWithHtmlAndText(backlog);

            foreach(var message in messages)
            {
                service.Backlog(message);
            }

            service.Start();
            service.Stop(DeliveryCancellationHandling.EmptyQueue);
            Assert.AreEqual(backlog, harness.Messages.Count);

            CleanDirectory(config.BacklogFolder);
        }

        [Test]
        public void Can_track_delivery_rate()
        {
            const int count = 100000;

            var config = new DeliveryConfiguration {BacklogFolder = "backlog", MaxDegreeOfParallelism = 10};

            var inMemory = new InMemoryEmailService();
            var service = new DeliveryService(inMemory, config);
            var messages = MessageFactory.EmailWithHtmlAndText(count);

            service.Start();
            service.Send(messages);
            service.Stop(DeliveryCancellationHandling.EmptyQueue);
            
            Assert.AreEqual(count, service.Delivered);
            Assert.AreEqual(count, inMemory.Messages.Count);

            Trace.WriteLine("Delivered: " + service.Delivered);
            Trace.WriteLine("Uptime: " + service.Uptime);
            Trace.WriteLine("Delivery rate: " + service.DeliveryRate + " msgs / second");
        }

        [Test]
        public void Can_cap_delivery_rate()
        {
            const int count = 2000;
            const int rate = 1000;

            var config = new DeliveryConfiguration { BacklogFolder = "backlog", MaxDegreeOfParallelism = 10, MaxDeliveryRate = rate };

            var inMemory = new InMemoryEmailService();
            var service = new DeliveryService(inMemory, config);
            var messages = MessageFactory.EmailWithHtmlAndText(count);

            service.Start();
            service.Send(messages);
            service.Stop(DeliveryCancellationHandling.EmptyQueue);

            Assert.AreEqual(count, service.Delivered);
            Assert.AreEqual(service.Delivered, inMemory.Messages.Count);
            Assert.IsTrue(service.DeliveryRate <= rate);

            Trace.WriteLine("Delivered: " + service.Delivered);
            Trace.WriteLine("Uptime: " + service.Uptime);
            Trace.WriteLine("Delivery rate: " + service.DeliveryRate + " msgs / second");
        }

        [Test]
        public void Can_schedule_delivery()
        {
            var config = new DeliveryConfiguration();
            
            var inMemory = new InMemoryEmailService();
            var service = new DeliveryService(inMemory, config);
            var messages = MessageFactory.EmailWithHtmlAndText(1).ToList();

            var message = messages.Single();
            message.DeliveryTime = 2.Seconds().FromNow();

            service.Start();
            service.Send(messages);
            Thread.Sleep(TimeSpan.FromSeconds(4));
            service.Stop(DeliveryCancellationHandling.EmptyQueue);

            Assert.AreEqual(1, service.Delivered);
        }
    }
}