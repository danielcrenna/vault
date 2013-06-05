using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    public class TwitterLocalTrends : TwitterTrends
    {
        private DateTime _createdDate;
        private IEnumerable<WhereOnEarthLocation> _locations;

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("CreatedDate")]
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
        public virtual IEnumerable<WhereOnEarthLocation> Locations
        {
            get { return _locations; }
            set
            {
                if (_locations == value)
                {
                    return;
                }

                _locations = value;
                OnPropertyChanged("Locations");
            }
        }

        public TwitterLocalTrends()
        {
            Initialize();
        }

        private void Initialize()
        {
            Locations = new List<WhereOnEarthLocation>(0);
        }
    }
}