using System;
using Hammock.Attributes.Specialized;
using Hammock.Web;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthWebQueryInfo : IWebQueryInfo
    {
        [Parameter("oauth_consumer_key")]
        public virtual string ConsumerKey { get; set; }

        [Parameter("oauth_token")]
        public virtual string Token { get; set; }

        [Parameter("oauth_nonce")]
        public virtual string Nonce { get; set; }

        [Parameter("oauth_timestamp")]
        public virtual string Timestamp { get; set; }

        [Parameter("oauth_signature_method")]
        public virtual string SignatureMethod { get; set; }

        [Parameter("oauth_signature")]
        public virtual string Signature { get; set; }

        [Parameter("oauth_version")]
        public virtual string Version { get; set; }

        [Parameter("oauth_callback")]
        public virtual string Callback { get; set; }

        [Parameter("oauth_verifier")]
        public virtual string Verifier { get; set; }

        [Parameter("x_auth_mode")]
        public virtual string ClientMode { get; set; }

        [Parameter("x_auth_username")]
        public virtual string ClientUsername { get; set; }

        [Parameter("x_auth_password")]
        public virtual string ClientPassword { get; set; }

        [UserAgent]
        public virtual string UserAgent { get; set; }

        public virtual WebMethod WebMethod { get; set; }

        public virtual OAuthParameterHandling ParameterHandling { get; set; }

        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }

        internal virtual string ConsumerSecret { get; set; }
        
        internal virtual string TokenSecret { get; set; }

        internal virtual bool FirstUse { get; set; }
    }
}