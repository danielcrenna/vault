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
    public class TwitterFriend : PropertyChangedBase, ITwitterModel
    {
        private long _id;
        private string _screenName;
        private bool _following;
        private bool _followedBy;
        private bool? _notificationsEnabled;
        private bool? _canDm;
        private bool? _blocking;

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
        public virtual bool Following
        {
            get { return _following; }
            set
            {
                if (_following == value)
                {
                    return;
                }

                _following = value;
                OnPropertyChanged("Following");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool FollowedBy
        {
            get { return _followedBy; }
            set
            {
                if (_followedBy == value)
                {
                    return;
                }

                _followedBy = value;
                OnPropertyChanged("FollowedBy");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual bool? NotificationsEnabled
        {
            get { return _notificationsEnabled; }
            set
            {
                if (_notificationsEnabled == value)
                {
                    return;
                }

                _notificationsEnabled = value;
                OnPropertyChanged("NotificationsEnabled");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("can_dm")]
        public virtual bool? CanDirectMessage
        {
            get { return _canDm; }
            set
            {
                if (_canDm == value)
                {
                    return;
                }

                _canDm = value;
                OnPropertyChanged("CanDirectMessage");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("blocking")]
        public virtual bool? IsBlocking
        {
            get { return _blocking; }
            set
            {
                if (_blocking == value)
                {
                    return;
                }

                _blocking = value;
                OnPropertyChanged("IsBlocking");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}