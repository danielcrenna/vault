namespace TweetSharp
{
    public class OAuthRequestToken
    {
        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
        public virtual bool OAuthCallbackConfirmed { get; set; }
    }
}
