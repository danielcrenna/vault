namespace TweetSharp
{
    public class OAuthAccessToken
    {
        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
        public virtual int UserId { get; set; }
        public virtual string ScreenName { get; set; }
    }
}