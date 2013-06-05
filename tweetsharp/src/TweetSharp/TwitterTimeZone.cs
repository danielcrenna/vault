using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
    //  "time_zone": {
    //    "name": "Pacific Time (US & Canada)",
    //    "tzinfo_name": "America/Los_Angeles",
    //    "utc_offset": -28800
    //  }
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
    [DebuggerDisplay("{Name}")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterTimeZone : PropertyChangedBase,
                                   IComparable<TwitterTimeZone>,
                                   IEquatable<TwitterTimeZone>,
                                   ITwitterModel
    {
        private string _name;
        private string _infoName;
        private short _utcOffset;

#if !Smartphone && !NET20
        [DataMember]
#endif
            public virtual string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        #if !Smartphone && !NET20
        [DataMember]
        [JsonProperty("tzinfo_name")]
#endif
            public virtual string InfoName
        {
            get { return _infoName; }
            set
            {
                if (_infoName == value)
                {
                    return;
                }

                _infoName = value;
                OnPropertyChanged("InfoName");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
            public virtual short UtcOffset
        {
            get { return _utcOffset; }
            set
            {
                if (_utcOffset == value)
                {
                    return;
                }

                _utcOffset = value;
                OnPropertyChanged("UtcOffset");
            }
        }

        public int CompareTo(TwitterTimeZone other)
        {
            return InfoName.CompareTo(other.InfoName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TwitterTimeZone)) return false;
            return Equals((TwitterTimeZone) obj);
        }

        public override int GetHashCode()
        {
            return (RawSource != null ? RawSource.GetHashCode() : 0);
        }

        public static bool operator ==(TwitterTimeZone left, TwitterTimeZone right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TwitterTimeZone left, TwitterTimeZone right)
        {
            return !Equals(left, right);
        }

        public bool Equals(TwitterTimeZone other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.RawSource, RawSource);
        }

#if !Smartphone && !NET20
       [DataMember]
#endif
       public virtual string RawSource { get; set; }
    }
}