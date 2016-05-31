using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ab.Tests
{
    [TestFixture]
    public class AudienceTests
    {
        [TestCase(10000, 2)]
        [TestCase(10000, 3)]
        [TestCase(10000, 4)]
        public void Splits_have_acceptable_distribution(int count, int n)
        {
            var items = new List<string>();
            for(var i = 0; i < count; i++)
            {
                items.Add(Guid.NewGuid().ToString());
            }

            var hash = new Dictionary<int, List<string>>(n);
            for(var i = 0; i < n; i++)
            {
                hash.Add(i + 1, new List<string>());
            }

            foreach(var item in items)
            {
                var index = Audience.Default.Value(item, n);
                hash[index].Add(item);
            }

            var perfect = ((float)count / n) / count;
            var total = 0;
            for(var i = 0; i < n; i++)
            {
                var entries = hash[i + 1].Count;
                total += entries;
                var percent = (float) entries/count;
                Console.WriteLine("Bucket {0} : {1}({2})", (i + 1), entries, percent);
                Assert.That(percent, Is.EqualTo(perfect).Within(0.5), "Distribution in bucket one was only " + percent);
            }
            Assert.AreEqual(count, total, "Not all items were distributed!");
        }
    }
}
