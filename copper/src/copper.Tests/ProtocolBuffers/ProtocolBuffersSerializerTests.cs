using NUnit.Framework;
using copper.ProtocolBuffers;

namespace copper.Tests.ProtocolBuffers
{
    [TestFixture]
    public class ProtocolBuffersSerializerTests
    {
        [Test]
        public void Serializes_and_deserializes_without_attributes()
        {
            var serializer = new ProtocolBuffersSerializer();
            var @event = new StringEvent("foo");
            var serialized = serializer.SerializeToStream(@event);
            serialized.Position = 0;
            var deserialized = serializer.DeserializeFromStream<StringEvent>(serialized);
            Assert.AreEqual(@event, deserialized);
        }
    }
}