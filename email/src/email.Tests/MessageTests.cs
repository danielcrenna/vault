using System.IO;
using NUnit.Framework;
using email.Providers;

namespace email.Tests
{
    public class MessageTests
    {
        protected static void AssertDelivery(EmailMessage message)
        {
            var pickupDirectory = Path.Combine(Path.GetTempPath(), "pickup");
            CreateOrCleanDirectory(pickupDirectory);

            var service = new DirectoryEmailProvider(pickupDirectory);
            service.Send(message);

            AssertFileCount(pickupDirectory, 1);
            CleanDirectory(pickupDirectory);
        }

        protected static void CreateOrCleanDirectory(string pickupDirectory)
        {
            if (!Directory.Exists(pickupDirectory))
            {
                Directory.CreateDirectory(pickupDirectory);
            }
            else
            {
                CleanDirectory(pickupDirectory);
            }
        }

        protected static void CleanDirectory(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }
        }

        protected static void AssertFileCount(string pickupDirectory, int count)
        {
            var files = Directory.GetFiles(pickupDirectory);
            Assert.AreEqual(count, files.Length);
        }
    }
}