using System;
using NUnit.Framework;
using cohort.Models;
using cohort.Services;
using cohort.Sqlite;

namespace cohort.Tests
{
    [TestFixture]
    public class UserRepositoryTests : SqliteDatabaseFixture
    {
        [Test]
        [Ignore("This currently breaks because we need to get database setup from Cohort itself, not from a fixture")]
        public void User_is_found_by_email()
        {
            var repository = new UserRepository(new SecurityService(), new RoleRepository());
            var user = new User
            {
                Email = "me@me.com",
                Password = "hash",
                Culture = "en-CA",
                JoinedOn = DateTime.Now
            };
            repository.Save(user);
            var fetched = repository.GetByEmail("me@me.com");
            Assert.AreEqual(user.Identity, fetched.Identity);
        }
    }
}
