using System;
using System.Diagnostics;
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
    [DebuggerDisplay("{ScreenName}")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterUser : PropertyChangedBase,
                               IComparable<TwitterUser>,
                               IEquatable<TwitterUser>,
                               ITweeter
    {
        private string _description;
        private int _followersCount;
        private long _id;
        private bool? _isProtected;
        private string _location;
        private string _name;
        private string _profileImageUrl;
        private string _screenName;
        private TwitterStatus _status;
        private string _url;
        private DateTime _createdDate;
        private bool? _isVerified;
        private bool? _isGeoEnabled;
        private bool _isProfileBackgroundTiled;
        private string _profileBackgroundColor;
        private string _profileBackgroundImageUrl;
        private string _profileLinkColor;
        private string _profileSidebarBorderColor;
        private string _profileSidebarFillColor;
        private string _profileTextColor;
        private int _statusesCount;
        private int _friendsCount;
        private int _favouritesCount;
        private int _listedCount;
        private string _timeZone;
        private string _utcOffset;
        private string _language;
        private bool? _followRequestSent;
        private bool? _isTranslator;
        private bool? _contributorsEnabled;
        private bool? _defaultProfile;
        private string _profileBackgroundImageUrlHttps;
        private string _profileImageUrlHttps;
        
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual long Id
        {
            get { return _id; }
            set
            {
                if (_id == value)
                {
                    return;
                }

                _id = value;
                OnPropertyChanged("Id");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int FollowersCount
        {
            get { return _followersCount; }
            set
            {
                if (_followersCount == value)
                {
                    return;
                }

                _followersCount = value;
                OnPropertyChanged("FollowersCount");
            }
        }

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
#endif
        public virtual string Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                {
                    return;
                }

                _description = value;
                OnPropertyChanged("Description");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileImageUrl
        {
            get { return _profileImageUrl; }
            set
            {
                if (_profileImageUrl == value)
                {
                    return;
                }

                _profileImageUrl = value;
                OnPropertyChanged("ProfileImageUrl");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Url
        {
            get { return _url; }
            set
            {
                if (_url == value)
                {
                    return;
                }

                _url = value;
                OnPropertyChanged("Url");
            }
        }

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
        public virtual string Location
        {
            get { return _location; }
            set
            {
                if (_location == value)
                {
                    return;
                }

                _location = value;
                OnPropertyChanged("Location");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual TwitterStatus Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                OnPropertyChanged("Status");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int FriendsCount
        {
            get { return _friendsCount; }
            set
            {
                if (_friendsCount == value)
                {
                    return;
                }

                _friendsCount = value;
                OnPropertyChanged("FriendsCount");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileBackgroundColor
        {
            get { return _profileBackgroundColor; }
            set
            {
                if (_profileBackgroundColor == value)
                {
                    return;
                }

                _profileBackgroundColor = value;
                OnPropertyChanged("ProfileBackgroundColor");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string UtcOffset
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

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileTextColor
        {
            get { return _profileTextColor; }
            set
            {
                if (_profileTextColor == value)
                {
                    return;
                }

                _profileTextColor = value;
                OnPropertyChanged("ProfileTextColor");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileBackgroundImageUrl
        {
            get { return _profileBackgroundImageUrl; }
            set
            {
                if (_profileBackgroundImageUrl == value)
                {
                    return;
                }

                _profileBackgroundImageUrl = value;
                OnPropertyChanged("ProfileBackgroundImageUrl");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string TimeZone
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
        public virtual int FavouritesCount
        {
            get { return _favouritesCount; }
            set
            {
                if (_favouritesCount == value)
                {
                    return;
                }

                _favouritesCount = value;
                OnPropertyChanged("FavouritesCount");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int ListedCount
        {
            get { return _listedCount; }
            set
            {
                if (_listedCount == value)
                {
                    return;
                }

                _listedCount = value;
                OnPropertyChanged("ListedCount");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileLinkColor
        {
            get { return _profileLinkColor; }
            set
            {
                if (_profileLinkColor == value)
                {
                    return;
                }

                _profileLinkColor = value;
                OnPropertyChanged("ProfileLinkColor");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int StatusesCount
        {
            get { return _statusesCount; }
            set
            {
                if (_statusesCount == value)
                {
                    return;
                }

                _statusesCount = value;
                OnPropertyChanged("StatusesCount");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileSidebarFillColor
        {
            get { return _profileSidebarFillColor; }
            set
            {
                if (_profileSidebarFillColor == value)
                {
                    return;
                }

                _profileSidebarFillColor = value;
                OnPropertyChanged("ProfileSidebarFillColor");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileSidebarBorderColor
        {
            get { return _profileSidebarBorderColor; }
            set
            {
                if (_profileSidebarBorderColor == value)
                {
                    return;
                }

                _profileSidebarBorderColor = value;
                OnPropertyChanged("ProfileSidebarBorderColor");
            }
        }

        [JsonProperty("profile_background_tile")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool IsProfileBackgroundTiled
        {
            get { return _isProfileBackgroundTiled; }
            set
            {
                if (_isProfileBackgroundTiled == value)
                {
                    return;
                }

                _isProfileBackgroundTiled = value;
                OnPropertyChanged("IsProfileBackgroundTiled");
            }
        }
        
        [JsonProperty("verified")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? IsVerified
        {
            get { return _isVerified; }
            set
            {
                if (_isVerified == value)
                {
                    return;
                }

                _isVerified = value;
                OnPropertyChanged("IsVerified");
            }
        }

        [JsonProperty("geo_enabled")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? IsGeoEnabled
        {
            get { return _isGeoEnabled; }
            set
            {
                if (_isGeoEnabled == value)
                {
                    return;
                }

                _isGeoEnabled = value;
                OnPropertyChanged("IsGeoEnabled");
            }
        }

        [JsonProperty("lang")]
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

        [JsonProperty("created_at")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                if (_createdDate == value)
                {
                    return;
                }

                _createdDate = value;
                OnPropertyChanged("CreatedDate");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? FollowRequestSent
        {
            get { return _followRequestSent; }
            set
            {
                if (_followRequestSent == value)
                {
                    return;
                }

                _followRequestSent = value;
                OnPropertyChanged("FollowRequestSent");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? IsTranslator
        {
            get { return _isTranslator; }
            set
            {
                if (_isTranslator == value)
                {
                    return;
                }

                _isTranslator = value;
                OnPropertyChanged("IsTranslator");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? ContributorsEnabled
        {
            get { return _contributorsEnabled; }
            set
            {
                if (_contributorsEnabled == value)
                {
                    return;
                }

                _contributorsEnabled = value;
                OnPropertyChanged("ContributorsEnabled");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileBackgroundImageUrlHttps
        {
            get { return _profileBackgroundImageUrlHttps; }
            set
            {
                if (_profileBackgroundImageUrlHttps == value)
                {
                    return;
                }

                _profileBackgroundImageUrlHttps = value;
                OnPropertyChanged("ProfileBackgroundImageUrlHttps");
            }
        }
        
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string ProfileImageUrlHttps
        {
            get { return _profileImageUrlHttps; }
            set
            {
                if (_profileImageUrlHttps == value)
                {
                    return;
                }

                _profileImageUrlHttps = value;
                OnPropertyChanged("ProfileImageUrlHttps");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("default_profile")]
        public virtual bool? IsDefaultProfile
        {
            get { return _defaultProfile; }
            set
            {
                if (_defaultProfile == value)
                {
                    return;
                }

                _defaultProfile = value;
                OnPropertyChanged("IsDefaultProfile");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }

        #region IComparable<TwitterUser> Members

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has the following meanings: 
        /// Less than zero: This object is less than the <paramref name="user"/> parameter.
        /// Zero: This object is equal to <paramref name="user"/>. 
        /// Greater than zero: This object is greater than <paramref name="user"/>.
        /// </returns>
        public int CompareTo(TwitterUser user)
        {
            return user.Id == Id ? 0 : user.Id > Id ? -1 : 1;
        }

        #endregion

        #region IEquatable<TwitterUser> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="user"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TwitterUser user)
        {
            if (ReferenceEquals(null, user))
            {
                return false;
            }
            if (ReferenceEquals(this, user))
            {
                return true;
            }
            return user.Id == Id;
        }

        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="user">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object user)
        {
            if (ReferenceEquals(null, user))
            {
                return false;
            }
            if (ReferenceEquals(this, user))
            {
                return true;
            }
            return user.GetType() == typeof (TwitterUser) && Equals((TwitterUser) user);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TwitterUser left, TwitterUser right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TwitterUser left, TwitterUser right)
        {
            return !Equals(left, right);
        }
    }
}