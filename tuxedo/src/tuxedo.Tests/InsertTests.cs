using System;
using NUnit.Framework;
using tuxedo.Tests.Models;

namespace tuxedo.Tests
{
    [TestFixture]
    public class InsertTests : TuxedoTests
    {
        [Test]
        public void Insert_one()
        {
            var query = Tuxedo.Insert(new User { Email = "good@domain.com"});
            Assert.AreEqual("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.AreEqual(1, query.Parameters.Count);
            Assert.AreEqual("good@domain.com", query.Parameters["@Email"]);
            Console.WriteLine(query);
        }
    }
}
