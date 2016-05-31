using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Text;
using email.Delivery;

namespace email.Tests.Delivery
{
    [TestFixture]
    public class DeliveryFolderObservableTests : MessageTests
    {
        private JsonSerializer<EmailMessage> _serializer;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _serializer = new JsonSerializer<EmailMessage>();
        }

        [Test]
        public void Can_watch_folder()
        {
            CreateOrCleanDirectory("pickup");
            
            var copied = 0;
            var observable = new DeliveryFolderObservable("pickup", "*.json", true);
            observable.Subscribe(e =>
            {
                Console.WriteLine(e.EventArgs.FullPath);
                copied++;
            });

            var emails = MessageFactory.EmailWithHtmlAndText(100);
            foreach(var email in emails)
            {
                var path = Path.Combine("pickup", email.Id + ".json");
                File.WriteAllText(path, _serializer.SerializeToString(email));
            }

            Assert.AreEqual(100, copied);

            CleanDirectory("pickup");
        }

        [Test]
        public void Can_subscribe_to_watch_folder_and_deliver_new_emails()
        {
            CreateOrCleanDirectory("pickup");

            var block = new ManualResetEvent(false);
            const int trials = 500;

            var inMemory = new InMemoryEmailService();
            var config = new DeliveryConfiguration();
            var service = new DeliveryService(inMemory, config);
            
            // Wire up delivery subject and start service
            var collection = new Subject<EmailMessage>();
            service.Send(collection);
            service.Start();
            
            // Shuttles files into the subject
            var handled = 0;
            var folder = new DeliveryFolderObservable("pickup", "*.json", true);
            folder.Subscribe(e =>
            {
                if (File.Exists(e.EventArgs.FullPath))
                {
                    string json = null;
                    var read = false;
                    while (!read)
                    {
                        try
                        {
                            json = File.ReadAllText(e.EventArgs.FullPath);
                            read = true;
                        }
                        catch
                        {
                            // The FileSystemWatcher suuuuuuucks
                        }
                    }

                    var message = _serializer.DeserializeFromString(json);
                    collection.OnNext(message);
                    handled++;

                    if(File.Exists(e.EventArgs.FullPath))
                    {
                        File.Delete(e.EventArgs.FullPath);
                        if (handled == trials)
                        {
                            block.Set();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Mayday! We delivered an email more than once!");
                        block.Set();
                    }
                }
            });
            
            // Meanwhile, back at the ranch, emails are a'brewin'...
            var queued = 0;
            Task.Factory.StartNew(() =>
            {
                var emails = MessageFactory.EmailWithHtmlAndText(trials);
                foreach (var email in emails)
                {
                    File.WriteAllText(Path.Combine("pickup", email.Id + ".json"), _serializer.SerializeToString(email));
                    queued++;
                }
            });
            
            block.WaitOne(); 
            Console.WriteLine(service.DeliveryRate);
            
            Assert.AreEqual(queued, inMemory.Messages.Count, "Could not keep up with demand");
            Assert.AreEqual(trials, inMemory.Messages.Count, "Did not deliver quota");

            Console.WriteLine(queued);
            CleanDirectory("pickup");
        }
    }
}