using System;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// Represents the relationship between two users on Twitter
    /// as they related to each <see cref="TwitterFriend" /> representation.
    /// </summary>
    [Serializable]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterRelationship : PropertyChangedBase, ITwitterModel
    {
        private TwitterFriend _source;
        private TwitterFriend _target;

#if !Smartphone && !NET20 
        /// <summary>
        /// Gets or sets the relative source of the relationship.
        /// </summary>
        /// <value>The source.</value>
        [DataMember]
#endif
        [JsonProperty("source")]
        public virtual TwitterFriend Source
        {
            get { return _source; }
            set
            {
                if (_source == value)
                {
                    return;
                }

                _source = value;
                OnPropertyChanged("Source");
            }
        }

#if !Smartphone && !NET20
        /// <summary>
        /// Gets or sets the relative target of the relationship.
        /// </summary>
        /// <value>The target.</value>
        [DataMember]
#endif
        [JsonProperty("target")]
        public virtual TwitterFriend Target
        {
            get { return _target; }
            set
            {
                if (_target == value)
                {
                    return;
                }

                _target = value;
                OnPropertyChanged("Target");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}