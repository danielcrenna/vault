using System;
using System.Collections;
using System.Collections.Generic;
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
    public class TwitterTrends : PropertyChangedBase, ITwitterModel, IEnumerable<TwitterTrend>
    {
        private List<TwitterTrend> _trends;

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual List<TwitterTrend> Trends
        {
            get { return _trends; }
            set
            {
                if (_trends == value)
                {
                    return;
                }

                _trends = value;
                OnPropertyChanged("Trends");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }

        public virtual IEnumerator<TwitterTrend> GetEnumerator()
        {
            return Trends.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TwitterTrends()
        {
            Initialize();
        }

        private void Initialize()
        {
            Trends = new List<TwitterTrend>(0);
        }
    }
}