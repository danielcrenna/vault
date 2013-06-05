using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Qualm.Supervised.Classification;

namespace Qualm.Tests.Serialization
{
    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void Can_serialize_matrix()
        {
            var matrix = new Matrix(new Number[,]
                    {
                        { 1.0, 1.1 },
                        { 1.0, 1.0 },
                        { 0.0, 0.0 },
                        { 0.0, 0.1 }
                    });

            var json = matrix.ToJson();
            AssertValidJson(json);
            Console.WriteLine(json);
        }

        [Test]
        public void Can_serialize_decision_tree()
        {
            var tree = DecisionTrees.ID3(new Matrix(new Number[,]
                                  {
                                      { 1, 1, 1 },    // YES
                                      { 1, 1, 1 },    // YES
                                      { 1, 0, 0 },    // NO
                                      { 0, 1, 0 },    // NO
                                      { 0, 1, 0 },    // NO
                                  }), new[] { "A", "B" });

            var json = tree.ToJson();
            AssertValidJson(json);
            Console.WriteLine(json);
        }

        private static void AssertValidJson(string json)
        {
            var deserialized = JsonConvert.DeserializeObject(json);
            Assert.IsNotNull(deserialized);
        }
    }
}
