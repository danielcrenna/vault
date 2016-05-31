using System;
using NUnit.Framework;
using tuxedo.Tests.Models;

namespace tuxedo.Tests
{
    [TestFixture]
    public class UpdateTests : TuxedoTests
    {
        [Test]
        public void Update_with_no_where()
        {
            var query = Tuxedo.Update<User>(new { Email = "good@domain.com" });
            Assert.AreEqual("UPDATE User SET Email = @Email_set", query.Sql);
            Assert.AreEqual(1, query.Parameters.Count);
            Assert.AreEqual("good@domain.com", query.Parameters["@Email_set"]);
            Console.WriteLine(query);
        }

        [Test]
        public void Update_with_explicit_where()
        {
            var query = Tuxedo.Update<User>(new { Email = "good@domain.com" }, new { Email = "bad@domain.com" });
            Assert.AreEqual("UPDATE User SET Email = @Email_set WHERE Email = @Email", query.Sql);
            Assert.AreEqual(2, query.Parameters.Count);
            Assert.AreEqual("bad@domain.com", query.Parameters["@Email"]);
            Assert.AreEqual("good@domain.com", query.Parameters["@Email_set"]);
            Console.WriteLine(query);
        }

        [Test]
        public void Update_with_instances()
        {
            var user1 = new User { Id = 1, Email = "good@email.com" };
            var user2 = new User { Id = 2, Email = "good@email.com" };
            var query = Tuxedo.Update<User>(new [] { user1, user2 });
            Assert.Fail();
        }
    }
}