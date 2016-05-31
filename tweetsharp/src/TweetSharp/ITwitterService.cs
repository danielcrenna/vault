using System;
#if !WINDOWS_PHONE
using System.Globalization;
#endif
using Hammock;
using Hammock.Serialization;

namespace TweetSharp
{
	public partial interface ITwitterService
	{
#if !WINDOWS_PHONE && !SILVERLIGHT && !WINRT
		void AuthenticateWith(string token, string tokenSecret);
		void AuthenticateWith(string consumerKey, string consumerSecret, string token, string tokenSecret);

		string GetEchoRequest(string url);
		Uri GetAuthorizationUri(OAuthRequestToken oauth);
		Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback);
		Uri GetAuthorizationUri(OAuthRequestToken oauth, CultureInfo culture);
		Uri GetAuthorizationUri(OAuthRequestToken oauth, string callback, CultureInfo culture);
		Uri GetAuthenticationUrl(OAuthRequestToken oauth);
		Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback);
		Uri GetAuthenticationUrl(OAuthRequestToken oauth, CultureInfo culture);
		Uri GetAuthenticationUrl(OAuthRequestToken oauth, string callback, CultureInfo culture);
		OAuthRequestToken GetRequestToken(string callback);
		OAuthRequestToken GetRequestToken();
		OAuthAccessToken GetAccessTokenWithXAuth(string username, string password);
		OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken);
		OAuthAccessToken GetAccessToken(OAuthRequestToken requestToken, string verifier);
		RestRequest PrepareEchoRequest(string realm = "http://api.twitter.com");
		IDeserializer Deserializer { get; set; }
		ISerializer Serializer { get; set; }
		T Deserialize<T>(ITwitterModel model) where T : ITwitterModel;
		T Deserialize<T>(string content);
		void CancelStreaming();
#endif
	}
}