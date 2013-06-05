using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
    /// <summary>
    /// Denotes a list of user friends in a user stream.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamFriends : TwitterStreamArtifact
    {
        private IEnumerable<long> _ids;
        
        [JsonProperty("friends")]
        public virtual IEnumerable<long> Ids
        {
            get { return _ids; }
            set
            {
                if (_ids == value)
                {
                    return;
                }

                _ids = value;
                OnPropertyChanged("Ids");
            }
        }
    }

    /// <summary>
    /// Denotes a <see cref="TwitterStatus" /> in a user stream.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamStatus : TwitterStreamArtifact, ITweetable
    {
        private TwitterStatus _status;

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

        public long Id
        {
            get { return Status.Id; }
        }

        public string Text
        {
            get { return Status.Text; }
        }

        public string TextAsHtml
        {
            get { return Status.TextAsHtml; }
        }

        public ITweeter Author
        {
            get { return Status.Author; }
        }

        public DateTime CreatedDate
        {
            get { return Status.CreatedDate; }
        }

        public TwitterEntities Entities
        {
            get { return Status.Entities; }
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamDirectMessage: TwitterStreamArtifact, ITweetable
    {
        private TwitterDirectMessage _dm;

        public virtual TwitterDirectMessage DirectMessage
        {
            get { return _dm; }
            set
            {
                if (_dm == value)
                {
                    return;
                }

                _dm = value;
                OnPropertyChanged("DirectMessage");
            }
        }

        public long Id
        {
            get { return DirectMessage.Id; }
        }

        public string Text
        {
            get { return DirectMessage.Text; }
        }

        public string TextAsHtml
        {
            get { return DirectMessage.TextAsHtml; }
        }

        public ITweeter Author
        {
            get { return DirectMessage.Author; }
        }

        public DateTime CreatedDate
        {
            get { return DirectMessage.CreatedDate; }
        }

        public TwitterEntities Entities
        {
            get { return DirectMessage.Entities; }
        }
    }

    /// <summary>
    /// Denotes a stream event, resulting from a user action.
    /// The source is always the initiating user, the target is always the affected user.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamEventBase : TwitterStreamArtifact
    {
        [JsonProperty("source")]
        public virtual TwitterUser Source { get; set; }

        [JsonProperty("target")]
        public virtual TwitterUser Target { get; set; }

        [JsonProperty("event")]
        public virtual string Event { get; set; }

        [JsonProperty("created_at")]
        public virtual DateTime CreatedDate { get; set; }
    }

    // Denotes a status deleted on a timeline in a user stream.
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamDeleteStatus : TwitterStreamArtifact
    {
        public virtual int UserId { get; set; }
        public virtual long StatusId { get; set; }
    }

    // Denotes a status deleted on a timeline in a user stream.
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamDeleteDirectMessage : TwitterStreamArtifact
    {
        public virtual int UserId { get; set; }
        public virtual long DirectMessageId { get; set; }
    }
    
    /// <summary>
    /// Denotes a stream event, resulting from a user action;
    /// the source is always the initiating user, the target is always the affected user, and
    /// the target object depends on the initiating action: statuses, direct messages, and lists.
    /// </summary>
    public class TwitterUserStreamEvent : TwitterUserStreamEventBase
    {
        public virtual ITwitterModel TargetObject { get; set; }

        public TwitterUserStreamEvent(TwitterUserStreamEventBase @base)
        {
            Source = @base.Source;
            Target = @base.Target;
            Event = @base.Event;
            CreatedDate = @base.CreatedDate;
            RawSource = @base.RawSource;
        }
    }

    /// <summary>
    /// Denotes the end of a user stream
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterUserStreamEnd : TwitterStreamArtifact
    {
        
    }
    
    /// <summary>
    /// Denotes content surfaced in a stream
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterStreamArtifact : PropertyChangedBase, ITwitterModel 
    {
        public virtual string RawSource { get; set; }
    }
}
