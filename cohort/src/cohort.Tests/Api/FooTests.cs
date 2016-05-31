using System.Net;
using NUnit.Framework;

namespace cohort.Tests.Api
{
    [TestFixture]
    public class FooTests : ApiFixture
    {
        [Test]
        public void Fails_when_accept_header_not_provided()
        {
            var response = Client.Get(BaseAddress, "api/foo");
            response.Is(HttpStatusCode.NotAcceptable);
        }

        [Test]
        public void Fails_when_no_authentication_provided()
        {
            var response = Client.Get(BaseAddress, "api/foo", message =>
            {
                message.Headers.Add("Accept", "application/json");
            });
            response.Is(HttpStatusCode.Unauthorized);
        }
    }
}