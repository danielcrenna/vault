using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Json.Tests
{
    [TestFixture]
    public class JsonParserTests
    {
        private static readonly char[] _base16 = new[]
                                             {
                                                 '0', '1', '2', '3', 
                                                 '4', '5', '6', '7', 
                                                 '8', '9', 'A', 'B', 
                                                 'C', 'D', 'E', 'F'
                                             };

        public class Dog
        {
            public string Name { get; set; }    
        }

        [Test]
        public void Can_parse_simple_example()
        {
            const string json = @"{ ""name"" : ""spot"" }";
            var dog = JsonParser.Deserialize<Dog>(json);
            Assert.IsTrue(dog.Name.Equals("spot"));
        }

        [Test]
        public void Can_parse_unicode_literals_and_symbols()
        {
            const string json = "{ \"literal\": \"\\u03a0\", \"symbol\": \"\x3a0\" }";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(bag["literal"], bag["symbol"]);
            Trace.WriteLine(bag["literal"]);
        }

        [Test]
        public void Can_parse_control_characters_as_whitespace()
        {
            const string json = "[\t\r\b\f\n{\"color\": \"red\",\"value\": \"#f00\"}]";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
        }

        [Test]
        public void Can_parse_strings_with_escaped_characters()
        {
            const string json = @"{""string"":""\""\/\\\b\f\n\r\t""}";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(1, bag.Count);
            Assert.True(bag.ContainsKey("string"));
            Assert.AreEqual("\"/\\\b\f\n\r\t", bag["string"]);
        }

        [Test]
        public void Can_parse_arrays()
        {
            const string json = @"[{""color"": ""red"",""value"": ""#f00""}]";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
        }

        [Test]
        public void Can_parse_keywords()
        {
            const string json = @"{ ""yay"" : true, ""nay"": false, ""nada"": null }";
            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(true, bag["yay"]);
            Assert.AreEqual(false, bag["nay"]);
            Assert.AreEqual(null, bag["nada"]);
        }

        [Test]
        public void Can_parse_numbers()
        {
            const string json = @"{""quantity"":8902,""cost"":45.33,""value"":-1.063E-02}";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(8902, bag["quantity"]);
            Assert.AreEqual(45.33, bag["cost"]);
            Assert.AreEqual(-1.063E-02, bag["value"]);
        }

        [Test]
        public void Can_serialize_simple_example()
        {
            const string expected = @"{""name"":""spot""}";
            var dog = new Dog { Name = "spot" };

            var actual = JsonParser.Serialize(dog);
            Assert.AreEqual(expected, actual);
        }

		[Test]
		public void Can_serialize_strings_with_characters_to_escape()
		{
			const string expected = @"{""name"":""Ba\""\/\\\b\f\n\r\tr""}";
			var dog = new Dog { Name = "Ba\"/\\\b\f\n\r\tr" };			
			var actual = JsonParser.Serialize(dog);
			Assert.AreEqual(expected, actual);
		}

        [Test]
        public void Can_serialize_with_numbers()
        {
            const string expected = @"{""quantity"":8902,""cost"":45.33,""value"":-0.01063}";
            var instance = new {
                                  quantity = 8902,
                                  cost = 45.33,
                                  value = -1.063E-02
                               };
            var actual = JsonParser.Serialize(instance);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_serialize_with_anonymous_types()
        {
            const string expected = @"{""array"":{""quantity"":8902,""cost"":45.33,""value"":-0.01063}}";
            var instance = new
            {
                array = new { quantity = 8902, cost = 45.33, value = -1.063E-02 }
            };
            var actual = JsonParser.Serialize(instance);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_serialize_with_dates()
        {
            var instance = new
            {
                now = DateTime.UtcNow
            };
            var actual = JsonParser.Serialize(instance);
            Trace.WriteLine(actual);
        }

        public class Time
        {
            public DateTime Now { get; set; }
        }

        [Test]
        public void Can_parse_dates()
        {
            var instance = new Time { Now = DateTime.UtcNow };

            var time = instance.Now;
            var lossy = new DateTime(time.Year,
                                     time.Month,
                                     time.Day,
                                     time.Hour,
                                     time.Minute,
                                     time.Second);

            var expected = JsonParser.Serialize(instance);
            var actual = JsonParser.Deserialize<Time>(expected);

            Assert.AreEqual(lossy, actual.Now);
        }

        [Test]
        public void Can_convert_to_base_16()
        {
            const int input = 67987356;

            var converted = JsonParser.BaseConvert(input, _base16, 4);
            Assert.AreEqual("40D679C", converted);
        }

        [Test]
        public void Can_convert_to_base_16_with_padding()
        {
            const int input = 24;

            var converted = JsonParser.BaseConvert(input, _base16, 4);
            Assert.AreEqual(converted.Length, 4);
            Assert.AreEqual("0018", converted);
        }

        [Test]
        public void Can_parse_with_unicode()
        {
            var dog = new Dog { Name = "Ăbbey" };

            var serialized = JsonParser.Serialize(dog);

            Console.WriteLine(serialized);

            var deserialized = JsonParser.Deserialize<Dog>(serialized);

            Console.WriteLine(deserialized.Name);
        }

        public class StringWrapper
        {
            public string Value { get; set; }
        }

        public class DogArray
        {
            public Dog[] Dogs { get; set; }
        }

        public class DogEnumerable
        {
            public IEnumerable<Dog> Dogs { get; set; }
        }

        public class DogList
        {
            public List<Dog> Dogs { get; set; }
        }

        [Test]
        public void Can_parse_typed_arrays()
        {
            var dogArray = JsonParser.Deserialize<DogArray>(@"{""dogs"":[{""name"":""dog0""},{""name"":""dog1""}]}");
            Assert.AreEqual(dogArray.Dogs.Length, 2);
            Assert.AreEqual(dogArray.Dogs[0].Name, "dog0");
            Assert.AreEqual(dogArray.Dogs[1].Name, "dog1");
        }
        
        [Test]
        public void Can_parse_typed_arrays_empty()
        {
            var dogArray = JsonParser.Deserialize<DogArray>(@"{""dogs"":[]}");
            Assert.AreEqual(dogArray.Dogs.Length, 0);
        }

        [Test]
        public void Can_parse_typed_enumerables()
        {
            var dogEnumerable = JsonParser.Deserialize<DogEnumerable>(@"{""dogs"":[{""name"":""dog0""},{""name"":""dog1""}]}");
            Assert.AreEqual(dogEnumerable.Dogs.Count(), 2);
            Assert.AreEqual(dogEnumerable.Dogs.First().Name, "dog0");
            Assert.AreEqual(dogEnumerable.Dogs.Last().Name, "dog1");
        }

        [Test]
        public void Can_parse_typed_enumerables_empty()
        {
            var dogEnumerable = JsonParser.Deserialize<DogEnumerable>(@"{""dogs"":[]}");
            Assert.AreEqual(dogEnumerable.Dogs.Count(), 0);
        }

        [Test]
        public void Can_parse_typed_lists()
        {
            var dogList = JsonParser.Deserialize<DogList>(@"{""dogs"":[{""name"":""dog0""},{""name"":""dog1""}]}");
            Assert.AreEqual(dogList.Dogs.Count, 2);
            Assert.AreEqual(dogList.Dogs[0].Name, "dog0");
            Assert.AreEqual(dogList.Dogs[1].Name, "dog1");
        }

        [Test]
        public void Can_parse_typed_lists_empty()
        {
            var dogList = JsonParser.Deserialize<DogList>(@"{""dogs"":[]}");
            Assert.AreEqual(dogList.Dogs.Count, 0);
        }


        public class DogPair
        {
            public Dog Dog1 { get; set; }
            public Dog Dog2 { get; set; }
        }

        [Test]
        public void Can_parse_nestedProperties()
        {
            var dogPair = JsonParser.Deserialize<DogPair>(@"{""dog1"":{""name"":""dog1""}, ""dog2"":{""name"":""dog2""}}");
            Assert.AreEqual(dogPair.Dog1.Name, "dog1");
            Assert.AreEqual(dogPair.Dog2.Name, "dog2");
        }

#if NET40
        [Test]
        public void Can_deserialize_simple_dynamic_object()
        {
            const string expected = @"{""array"":{""quantity"":8902,""cost"":45.33,""value"":-0.01063}}";
            var instance = JsonParser.Deserialize(expected);
            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.array);
            Assert.IsNotNull(instance.Array);
            Assert.IsNotNull(instance.array.quantity);
        }

        [Test]
        public void Can_deserialize_simple_dynamic_object_collection()
        {
            const string expected = @"[
            { ""quantity"":8902,""cost"":45.33,""value"":-0.01063 },
            { ""quantity"":8903,""cost"":45.34,""value"":-0.01064 }
            ]";

            var collection = JsonParser.Deserialize(expected);

            foreach(var item in collection)
            {
                Assert.IsNotNull(item);
                Assert.IsNotNull(item.quantity);
                Assert.IsNotNull(item.Quantity);
                Assert.IsNotNull(item.cost);
                Assert.IsNotNull(item.Cost);
                Assert.IsNotNull(item.value);
                Assert.IsNotNull(item.Value);
            }
        }
#endif
    }
}
