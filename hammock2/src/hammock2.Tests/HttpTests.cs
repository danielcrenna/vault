using System;
using System.Net;
using NUnit.Framework;
using hammock2.Tests.API;

namespace hammock2.Tests
{
    // TODO
    // Test for basic auth
    // Test to confirm query string received
    // Test POST entities

    [TestFixture]
    public class HttpTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            FakeServer.Configure(FakeController.RegisterRoutes);
            HttpClientEngine.ClientFactory = () => FakeServer.CreateClientForAServerOnPort(8787);
        }

        [Test]
        public void Can_define_headers()
        {
            var http = DynamicHttp();
            http.Headers.Add("bob", "loblaw");
            Assert.AreEqual(1, http.Headers.Count);
        }

        [Test]
        public void Request_parameterless_get()
        {
            var http = DynamicHttp();
            var result = http();
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNull(result.Body);
        }

        [Test]
        public void When_resource_not_found_hashes_are_safely_traversed()
        {
            var query = DynamicHttp();
            var reply = query.This.Is.Totally.Not.A.Url();
            Assert.AreEqual(HttpStatusCode.NotFound, reply.Response.StatusCode);
            var body = reply.Body;
            var nonsense = body.Definitely.No.Property.Structure.Such.As.This;
            Assert.AreEqual(nonsense, body.Null);
        }

        [Test]
        public void Can_get_an_entity_from_a_url_on_first_node()
        {
            var query = DynamicHttp();
            var reply = query.One(screen_name: "danielcrenna");
            var user = reply.Body;
            Assert.IsNotNull(user);
            Assert.AreEqual("danielcrenna", user.ScreenName);
            Console.WriteLine(user.ScreenName + ":" + user.Status.Text);
        }

        [Test]
        public void Can_get_an_entity_from_a_url_on_second_node()
        {
            var query = DynamicHttp();
            var reply = query.One.Two(screen_name: "danielcrenna");
            var user = reply.Body;
            Assert.IsNotNull(user);
            Assert.AreEqual("danielcrenna", user.ScreenName);
            Console.WriteLine(user.ScreenName + ":" + user.Status.Text);
        }

        [Test]
        public void Can_get_an_entity_from_a_url_on_dot_node()
        {
            var query = DynamicHttp();
            var reply = query.One.Two.Three.Dot.Four(screen_name: "danielcrenna");
            var user = reply.Body;
            Assert.IsNotNull(user);
            Assert.AreEqual("danielcrenna", user.ScreenName);
            Console.WriteLine(user.ScreenName + ":" + user.Status.Text);
        }

        private static dynamic DynamicHttp()
        {
            dynamic http = new Http("http://localhost:8787");
            http.Trace = true;
            return http;
        }
    }
}
