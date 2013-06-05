using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace protobuf.Poco.Tests
{
    [TestFixture]
    public class ProtocolBuffersSerializerTests
    {
        [Test]
        public void Serializes_and_deserializes_without_attributes()
        {
            var serializer = new Serializer();
            var user = new User();
            user.Id = 1;
            user.Email = "good@domain.com";
            user.Image = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            user.CreatedAt = DateTime.Now;
            user.Tags = new List<string>();
            user.Tags.Add("soccer");

            var serialized = serializer.SerializeToStream(user);
            serialized.Position = 0;
            var deserialized = serializer.DeserializeFromStream<User>(serialized);
            Assert.AreEqual(user.Id, deserialized.Id);
            Assert.AreEqual(user.Email, deserialized.Email);
            Assert.AreEqual(user.Image, deserialized.Image);
            Assert.AreEqual(user.CreatedAt, deserialized.CreatedAt);
            Assert.AreEqual(user.Tags, deserialized.Tags);
        }
    }
}