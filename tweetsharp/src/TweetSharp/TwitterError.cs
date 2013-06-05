using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
    [DebuggerDisplay("{Code}: {Message}")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterError : ITwitterModel
    {       
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int Code { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Message { get; set; }
        
        public override string ToString()
        {
            return string.Format("{0}: {1}", Code, Message);
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}