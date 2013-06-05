using System;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterTrend : PropertyChangedBase, ITwitterModel
    {
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Name { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Url { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Query { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Events { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string PromotedContent { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual DateTime TrendingAsOf { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}