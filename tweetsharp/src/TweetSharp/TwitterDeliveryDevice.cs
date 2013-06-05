using System;
using System.Runtime.Serialization;

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// Represents a delivery device for receiving updates from Twitter.
    /// </summary>
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public enum TwitterDeliveryDevice
    {
#if !Smartphone && !NET20
        /// <summary>
        /// No delivery device is used.
        /// </summary>
        [EnumMember] None,
        /// <summary>
        /// An SMS-capable delivery device is used.
        /// </summary>
        [EnumMember] Sms
#else
        None,
        Sms
#endif
    }
}