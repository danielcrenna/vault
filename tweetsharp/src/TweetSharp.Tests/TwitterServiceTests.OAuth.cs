using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using Hammock;
using NUnit.Framework;

namespace TweetSharp.Tests.Service
{
    public partial class TwitterServiceTests
    {
        [Test]
        public void Can_get_request_token()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            var requestToken = service.GetRequestToken();
            
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.NotNull(requestToken);
        }

        [Test]
        [Ignore("Not a sequential test.")]
        public void Can_exchange_for_access_token()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            var requestToken = service.GetRequestToken();

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.NotNull(requestToken);

            var uri = service.GetAuthorizationUri(requestToken);
            Process.Start(uri.ToString());

            Console.WriteLine("Press the any key when you have confirmation of your code transmission.");
            var verifier = "1234567"; // <-- Debugger breakpoint and edit with the actual verifier

            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, verifier);
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(accessToken);
        }

        [Test]
        [Ignore("Not a sequential test.")]
        public void Can_make_protected_resource_request_with_access_token()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            var request = service.GetRequestToken();

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.NotNull(request);

            var uri = service.GetAuthorizationUri(request);
            Process.Start(uri.ToString());

            Console.WriteLine("Press the any key when you have confirmation of your code transmission.");
            var verifier = "1234567"; // <-- Debugger breakpoint and edit with the actual verifier

            var access = service.GetAccessToken(request, verifier);
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(access);

            service.AuthenticateWith(access.Token, access.TokenSecret);
            var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions());
            Assert.IsNotNull(mentions);
            Assert.AreEqual(20, mentions.Count());
        }

        [Test]
        public void Can_tweet_with_protected_resource_info()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);
            var status = service.SendTweet(new SendTweetOptions { Status = DateTime.Now.Ticks.ToString() });
            Assert.IsNotNull(status);
        }

        [Test]
        public void Can_make_xauth_request()
        {
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            OAuthAccessToken access = service.GetAccessTokenWithXAuth("username", "password");
            Assert.Ignore("Test account isn't authorized for xAuth");
        }

				//Remmoved as TwitPic is now defunct and I haven't figured out another service to use instead.
				//[Test]
				//public void Can_make_oauth_echo_request()
				//{
				//	if (String.IsNullOrEmpty(_twitPicKey)) Assert.Ignore("This test requires a TwitPic API key.");
				//	if (String.IsNullOrEmpty(_twitPicUserName)) Assert.Ignore("This test requires a TwitPic user name.");
					
				//	var service = GetAuthenticatedService();
				//	var request = service.PrepareEchoRequest("http://api.twitpic.com/2/");
				//	request.Path = "users/show.json?username=" + HttpUtility.UrlEncode(_twitPicUserName);
				//	request.AddField("key", _twitPicKey);

				//	RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "2" };
				//	RestResponse response = client.Request(request);

				//	Assert.IsNotNull(response);
				//	Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				//}

        [Test]
        public void Can_verify_credentials()
        {
            var service = GetAuthenticatedService();
            var user = service.VerifyCredentials(new VerifyCredentialsOptions());
            Assert.IsNotNull(user);
        }
    }
}
