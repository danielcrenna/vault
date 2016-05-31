using System;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthToken
    {
        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
        public virtual string Verifier { get; set; }
        public virtual bool CallbackConfirmed { get; set; }
        public virtual string UserId { get; set; }
        public virtual string ScreenName { get; set; }
    }
}