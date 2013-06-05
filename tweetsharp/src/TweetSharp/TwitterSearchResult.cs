using System;
using System.Collections.Generic;
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
    [DebuggerDisplay("{Statuses}")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterSearchResult : ITwitterModel
    {
        [JsonProperty("statuses")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual IEnumerable<TwitterStatus> Statuses { get; set; }
        
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}