using System.Configuration;
using System.Net;
using NUnit.Framework;

namespace OAuth.Tests
{
    [TestFixture]
    public class OAuthTests
    {
        private const string BaseUrl = "http://twitter.com/oauth/{0}";

        private string _consumerKey;
        private string _consumerSecret;

        [SetUp]
        public void SetUp()
        {
            _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        }

        [Test]
        public void Can_get_request_token_with_http_header()
        {
            var client = new OAuthRequest
                             {
                                 Method = "GET",
                                 ConsumerKey = _consumerKey,
                                 ConsumerSecret = _consumerSecret,
                                 RequestUrl = string.Format(BaseUrl, "request_token")
                             };

            var auth = client.GetAuthorizationHeader();
            
            var request = (HttpWebRequest) WebRequest.Create(client.RequestUrl);
            
            request.Headers.Add("Authorization", auth);

            var response = (HttpWebResponse) request.GetResponse();

            Assert.IsNotNull(response);

            Assert.AreEqual(200, (int)response.StatusCode);
        }

        [Test]
        public void Can_get_request_token_with_query()
        {
            var client = OAuthRequest.ForRequestToken(_consumerKey, _consumerSecret);
            
            client.RequestUrl = string.Format(BaseUrl, "request_token");

            var auth = client.GetAuthorizationQuery();

            var url = client.RequestUrl + "?" + auth;

            var request = (HttpWebRequest)WebRequest.Create(url);
            
            var response = (HttpWebResponse)request.GetResponse();

            Assert.IsNotNull(response);

            Assert.AreEqual(200, (int)response.StatusCode);
        }
    }
}
