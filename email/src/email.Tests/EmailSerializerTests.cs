using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Text;

namespace email.Tests
{
    [TestFixture]
    public class EmailSerializerTests
    {
        [Test]
        public void Can_serialize_and_deserialize_text_email()
        {
            var serializer = new JsonSerializer<EmailMessage>();
            var email = MessageFactory.EmailWithText(1).Single();
            var json = serializer.SerializeToString(email);
            Assert.IsNotNull(json);
            
            Console.WriteLine(json);

            var deserialized = serializer.DeserializeFromString(json);
            Assert.IsNotNull(deserialized);
        }
    }
}