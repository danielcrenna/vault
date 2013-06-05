using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
    // Documentation: https://dev.twitter.com/docs/api/1/get/account/settings

#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterAccount : PropertyChangedBase,
                                  IComparable<TwitterAccount>,
                                  IEquatable<TwitterAccount>,
                                  ITwitterModel
    {
        private bool? _isProtected;
        private bool? _geoEnabled;
        private bool? _alwaysUseHttps;
        private bool? _discoverableByEmail;
        private string _language;
        private IEnumerable<WhereOnEarthLocation> _trendLocations;
        private string _screenName;
        private TwitterTimeZone _timeZone;
        private TwitterSleepTime _sleepTime;

        [JsonProperty("protected")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? IsProtected
        {
            get { return _isProtected; }
            set
            {
                if (_isProtected == value)
                {
                    return;
                }

                _isProtected = value;
                OnPropertyChanged("IsProtected");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? GeoEnabled
        {
            get { return _geoEnabled; }
            set
            {
                if (_geoEnabled == value)
                {
                    return;
                }

                _geoEnabled = value;
                OnPropertyChanged("GeoEnabled");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? AlwaysUseHttps
        {
            get { return _alwaysUseHttps; }
            set
            {
                if (_alwaysUseHttps == value)
                {
                    return;
                }

                _alwaysUseHttps = value;
                OnPropertyChanged("AlwaysUseHttps");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? DiscoverableByEmail
        {
            get { return _discoverableByEmail; }
            set
            {
                if (_discoverableByEmail == value)
                {
                    return;
                }

                _discoverableByEmail = value;
                OnPropertyChanged("DiscoverableByEmail");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Language
        {
            get { return _language; }
            set
            {
                if (_language == value)
                {
                    return;
                }

                _language = value;
                OnPropertyChanged("Language");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("trend_location")]
        public virtual IEnumerable<WhereOnEarthLocation> TrendLocations
        {
            get { return _trendLocations; }
            set
            {
                if (_trendLocations == value)
                {
                    return;
                }

                _trendLocations = value;
                OnPropertyChanged("TrendLocations");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
            public virtual string ScreenName
        {
            get { return _screenName; }
            set
            {
                if (_screenName == value)
                {
                    return;
                }

                _screenName = value;
                OnPropertyChanged("ScreenName");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual TwitterTimeZone TimeZone
        {
            get { return _timeZone; }
            set
            {
                if (_timeZone == value)
                {
                    return;
                }

                _timeZone = value;
                OnPropertyChanged("TimeZone");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual TwitterSleepTime SleepTime
        {
            get { return _sleepTime; }
            set
            {
                if (_sleepTime == value)
                {
                    return;
                }

                _sleepTime = value;
                OnPropertyChanged("SleepTime");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }

        public int CompareTo(TwitterAccount other)
        {
            return other.ScreenName.CompareTo(ScreenName);
        }

        public bool Equals(TwitterAccount other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.RawSource, RawSource);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TwitterAccount)) return false;
            return Equals((TwitterAccount) obj);
        }

        public override int GetHashCode()
        {
            return (RawSource != null ? RawSource.GetHashCode() : 0);
        }

        public static bool operator ==(TwitterAccount left, TwitterAccount right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TwitterAccount left, TwitterAccount right)
        {
            return !Equals(left, right);
        }
    }
}