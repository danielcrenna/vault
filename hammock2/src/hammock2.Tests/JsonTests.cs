using System;
using NUnit.Framework;

namespace hammock2.Tests
{
    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void Can_serialize_dynamic_instance()
        {
            var dog = new { Name = "Spot" };
            var json = HttpBody.Serialize(dog);
            
            Assert.IsNotNull(json);
            Console.WriteLine(json);
        }

        [Test]
        public void Can_deserialize_dynamic_instance()
        {
            var dog = new { Name = "Spot" };
            var json = HttpBody.Serialize(dog);
            var deserialized = HttpBody.Deserialize(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(dog.Name, deserialized.Name);
        }

        [Test]
        public void Can_deserialize_dynamic_instance_into_concrete_instance()
        {
            var dog = new { Name = "Spot" };
            var json = HttpBody.Serialize(dog);
            var deserialized = HttpBody.Deserialize(json);
            Dog concrete = HttpBody.Deserialize<Dog>(deserialized);
            Assert.IsNotNull(concrete);
            Assert.AreEqual("Spot", concrete.Name);
        }

        public class Dog
        {
            public string Name { get; set; }
        }
    }
}
