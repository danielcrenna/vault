using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// Represents a suggested user category from the Twitter API. 
    /// </summary>
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterUserSuggestions : PropertyChangedBase,
                                          IComparable<TwitterUserSuggestions>,
                                          IEquatable<TwitterUserSuggestions>, 
                                          ITwitterModel
    {
        private string _name;
        private string _slug;
        private int _size;
        private IEnumerable<TwitterUser> _users;

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
        public virtual string Slug
        {
            get { return _slug; }
            set
            {
                if (_slug == value)
                {
                    return;
                }

                _slug = value;
                OnPropertyChanged("Slug");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int Size
        {
            get { return _size; }
            set
            {
                if (_size == value)
                {
                    return;
                }

                _size = value;
                OnPropertyChanged("Size");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual IEnumerable<TwitterUser> Users
        {
            get { return _users; }
            set
            {
                if (_users == value)
                {
                    return;
                }

                _users = value;
                OnPropertyChanged("Users");
            }
        }

        public virtual string RawSource { get; set; }

        public virtual int CompareTo(TwitterUserSuggestions other)
        {
            return Slug.CompareTo(other.Slug);
        }

        public virtual bool Equals(TwitterUserSuggestions status)
        {
            if (ReferenceEquals(null, status)) return false;
            return ReferenceEquals(this, status) || Equals(status._slug, _slug);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (TwitterUserSuggestions) && Equals((TwitterUserSuggestions) obj);
        }

        public override int GetHashCode()
        {
            return (_slug != null ? _slug.GetHashCode() : 0);
        }
    }
}
