using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Json.Tests
{
#if NET40
    [TestFixture]
    public class JsonObjectTests
    {
        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }


        [Test]
        public void Can_convert_to_type()
        {
            var bag = JsonParser.InitializeBag();
            bag["name"] = "Bob";
            bag["age"] = 21;

            var jsonObject = new JsonObject(bag);
            var person1 = jsonObject.Convert<Person>();
            var person2 = jsonObject.Convert(typeof(Person)) as Person;
            dynamic jsonObject3 = jsonObject;
            var person3 = (Person)jsonObject3;

            Assert.IsNotNull(person1);
            Assert.AreEqual("Bob", person1.Name);
            Assert.AreEqual(21, person1.Age);
            Assert.IsNotNull(person2);
            Assert.AreEqual("Bob", person2.Name);
            Assert.AreEqual(21, person2.Age);
            Assert.IsNotNull(person3);
            Assert.AreEqual("Bob", person3.Name);
            Assert.AreEqual(21, person3.Age);
        }
    }
#endif
}
