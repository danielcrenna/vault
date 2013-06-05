using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Hammock.Tasks;
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
    public class TwitterRateLimitStatusSummary 
    {
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string AccessToken { get; set; }
        
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual List<TwitterRateLimitResource> Resources { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterRateLimitResource
    {
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Name { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual Dictionary<string, TwitterRateLimitStatus> Limits { get; set; }
    }

    
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
    [DebuggerDisplay("{RemainingHits} / {HourlyLimit} remaining.")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterRateLimitStatus :
        PropertyChangedBase,
        IComparable<TwitterRateLimitStatus>,
        IEquatable<TwitterRateLimitStatus>,
        IRateLimitStatus,
        ITwitterModel
    {
        private int _remainingHits;
        private int _hourlyLimit;
        private long _resetTimeInSeconds;
        private DateTime _resetTime;

        /// <summary>
        /// Gets or sets the remaining API hits allowed.
        /// </summary>
        /// <value>The remaining API hits allowed.</value>
        [JsonProperty("remaining_hits")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int RemainingHits
        {
            get { return _remainingHits; }
            set
            {
                if (_remainingHits == value)
                {
                    return;
                }

                _remainingHits = value;
                OnPropertyChanged("RemainingHits");
            }
        }

        /// <summary>
        /// Gets or sets the API hits hourly limit.
        /// You can compare this to <see cref="RemainingHits" /> to get a 
        /// percentage of usage remaining.
        /// </summary>
        /// <value>The hourly limit.</value>
        [JsonProperty("hourly_limit")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int HourlyLimit
        {
            get { return _hourlyLimit; }
            set
            {
                if (_hourlyLimit == value)
                {
                    return;
                }

                _hourlyLimit = value;
                OnPropertyChanged("HourlyLimit");
            }
        }

        /// <summary>
        /// Gets or sets the UNIX time representing the time
        /// this rate limit will reset.
        /// This is not the number of seconds until the rate limit
        /// resets.
        /// </summary>
        /// <value>The reset time in seconds.</value>
        [JsonProperty("reset_time_in_seconds")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual long ResetTimeInSeconds
        {
            get { return _resetTimeInSeconds; }
            set
            {
                if (_resetTimeInSeconds == value)
                {
                    return;
                }

                _resetTimeInSeconds = value;
                OnPropertyChanged("ResetTimeInSeconds");
            }
        }

        /// <summary>
        /// Gets or sets the reset time for this rate limit constraint.
        /// </summary>
        /// <value>The reset time.</value>
        [JsonProperty("reset_time")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual DateTime ResetTime
        {
            get { return _resetTime; }
            set
            {
                if (_resetTime == value)
                {
                    return;
                }

                _resetTime = value;
                OnPropertyChanged("ResetTime");
            }
        }

        #region Implementation of IComparable<TwitterRateLimitStatus>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(TwitterRateLimitStatus other)
        {
            return other.HourlyLimit.CompareTo(HourlyLimit) == 0 &&
                   other.ResetTime.CompareTo(ResetTime) == 0 &&
                   other.RemainingHits.CompareTo(RemainingHits) == 0
                       ? 0
                       : 1;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (TwitterRateLimitStatus) &&
                   Equals((TwitterRateLimitStatus) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = _remainingHits;
                result = (result*397) ^ _hourlyLimit;
                result = (result*397) ^ _resetTimeInSeconds.GetHashCode();
                result = (result*397) ^ _resetTime.GetHashCode();
                return result;
            }
        }

        #endregion

        #region Implementation of IEquatable<TwitterRateLimitStatus>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TwitterRateLimitStatus other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other._remainingHits == _remainingHits &&
                   other._hourlyLimit == _hourlyLimit &&
                   other._resetTimeInSeconds == _resetTimeInSeconds &&
                   other._resetTime.Equals(_resetTime);
        }

        #endregion

        #region IRateLimitStatus Members

        /// <summary>
        /// Gets the next reset time.
        /// </summary>
        /// <value>The next reset time.</value>
        DateTime IRateLimitStatus.NextReset
        {
            get { return ResetTime; }
        }

        /// <summary>
        /// Gets the remaining API uses.
        /// </summary>
        /// <value>The remaining API uses.</value>
        int IRateLimitStatus.RemainingUses
        {
            get { return RemainingHits; }
        }

        #endregion

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}