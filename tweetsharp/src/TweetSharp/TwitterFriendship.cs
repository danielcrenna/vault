using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
    /* {"relationship": {
            "source": {
                "id": 123,
                "screen_name": "bob",
                "following": true,
                "followed_by": false,
                "notifications_enabled": false
     *      }
     *      ,
            "target": {
                "id": 456,
                "screen_name": "jack",
                "following": false,
                "followed_by": true,
                "notifications_enabled": null
     *      }
     *   }
     * }
     */

#if !SILVERLIGHT
    [Serializable]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterFriendship : PropertyChangedBase, ITwitterModel
    {
        private TwitterRelationship _relationship;

#if !Smartphone && !NET20
        /// <summary>
        /// Gets or sets the relationship.
        /// </summary>
        /// <value>The relationship.</value>
        [DataMember]
#endif
        [JsonProperty("relationship")]
        public virtual TwitterRelationship Relationship
        {
            get { return _relationship; }
            set
            {
                if (_relationship == value)
                {
                    return;
                }

                _relationship = value;
                OnPropertyChanged("Relationship");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }

/* 
{
    "name": "Twitter",
    "id_str": "783214",
    "id": 783214,
    "connections": [
        "following",
        "followed_by"
    ],  
    "screen_name": "twitter"
}
*/
#if !SILVERLIGHT
    [Serializable]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Name}:{string.Join(\",\", Connections)}")]
    public class TwitterFriendshipLookup : PropertyChangedBase, ITwitterModel
    {
        private string _name;
        private int _id;
        private ICollection<string> _connections;
        private string _screenName;
    
        public bool Following
        {
            get { return _connections.Contains("following"); }
        }

        public bool FollowingRequested
        {
            get { return _connections.Contains("following_requested"); }
        }

        public bool FollowedBy
        {
            get { return _connections.Contains("followed_by"); }
        }

        public bool None
        {
            get { return _connections.Contains("none"); }
        }

        public TwitterFriendshipLookup()
        {
            _connections = new List<string>(4);
        }
        
        [JsonProperty("name")]
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

        [JsonProperty("connections")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual ICollection<string> Connections
        {
            get { return _connections; }
            set
            {
                if (_connections == value)
                {
                    return;
                }
                _connections = value;
                OnPropertyChanged("Connections");
            }
        }

        [JsonProperty("id")]
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual int Id
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

        [JsonProperty("screen_name")]
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
        public virtual string RawSource { get; set; }
    }

}