using System.IO;
using NUnit.Framework;
using copper.Json;
using copper.MsgPack;
using copper.ProtocolBuffers;

namespace copper.Tests
{
    [TestFixture]
    public class FileConsumerTests
    {
        [Test]
        public void Events_persist_as_binary_on_disk()
        {
            PersistsAsSerialized(new BinarySerializer(), ".dat");
        }

        [Test]
        public void Events_persist_as_xml_on_disk()
        {
            PersistsAsSerialized(new XmlSerializer(), ".xml");
        }

        [Test]
        public void Events_persist_as_json_on_disk()
        {
            PersistsAsSerialized(new JsonSerializer(), ".json");
        }

        [Test]
        public void Events_persist_as_msgpack_on_disk()
        {
            PersistsAsSerialized(new MsgPackSerializer(), ".msgpack");
        }

        [Test]
        public void Events_persist_as_protocol_buffers_on_disk()
        {
            PersistsAsSerialized(new ProtocolBuffersSerializer(), ".proto");
        }

        private static void PersistsAsSerialized(Serializer serializer, string extension)
        {
            var consumer = new FileConsumer<StringEvent>(serializer, "Events", extension);
            var @event = new StringEvent("Test!");
            consumer.Handle(@event);

            var file = OneFileSaved(extension);
            FileContainsTheEvent(file, serializer, @event);
        }

        private static void FileContainsTheEvent<T>(string file, Serializer serializer, T @event)
        {
            Assert.DoesNotThrow(() =>
            {
                var deserialized = serializer.DeserializeFromStream<StringEvent>(File.OpenRead(file));
                Assert.IsNotNull(deserialized);
                Assert.AreEqual(@event, deserialized);
            });
        }

        private static string OneFileSaved(string extension)
        {
            var files = Directory.GetFiles("Events", "*" + extension);
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            return file;
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (!Directory.Exists("Events"))
            {
                Directory.CreateDirectory("Events");
            }
            foreach (var file in Directory.GetFiles("Events", "*.*"))
            {
                File.Delete(file);
            }
        }
    }
}