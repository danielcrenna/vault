using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        [Ignore("Makes a live status update")]
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

        [Test]
        [Ignore("This test requires a TwitPic API key")]
        public void Can_make_oauth_echo_request()
        {
            var service = GetAuthenticatedService();
            var response = service.GetEchoRequest("http://api.twitpic.com/2/users/show.json?username=danielcrenna");
            Assert.IsNotNull(response);
            AssertResultWas(service, HttpStatusCode.OK);
        }

        [Test]
        public void Can_verify_credentials()
        {
            var service = GetAuthenticatedService();
            var user = service.VerifyCredentials(new VerifyCredentialsOptions());
            Assert.IsNotNull(user);
        }
    }
}
