using System;
using Dates;
using NUnit.Framework;

namespace linger.Tests
{
    [TestFixture]
    public class RepeatInfoTests
    {
        [Test]
        public void Is_serializable()
        {
            var repeatInfo = new RepeatInfo(DateTime.Now, DatePeriod.Daily);
            var json = JsonSerializer.Serialize(repeatInfo);
            Assert.IsNotNullOrEmpty(json);
            Console.WriteLine(json);
        }
    }
}