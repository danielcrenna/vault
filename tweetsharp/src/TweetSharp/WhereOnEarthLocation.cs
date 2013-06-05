using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;
using TweetSharp.Model;

namespace TweetSharp
{
    // {
    //     "url":"http://where.yahooapis.com/v1/place/23424900",
    //     "woeid":23424900,
    //     "placeType":{"code":12,"name":"Country"},
    //     "countryCode":"MX",
    //     "name":"Mexico",
    //     "country":"Mexico" }

#if !SILVERLIGHT
    /// <summary>
    /// Represents a location in the Yahoo! WOE specification.
    /// </summary>
    /// <seealso>"http://developer.yahoo.com/geo/geoplanet/"</seealso>
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
#if !Smartphone
    [DebuggerDisplay("{WoeId}: {Name}")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class WhereOnEarthLocation : PropertyChangedBase, ITwitterModel 
    {
        private long _woeId;
        private string _url;
        private string _name;
        private string _countryCode;
        private string _country;
        private WhereOnEarthPlaceType _placeType;

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("woeid")]
        public virtual long WoeId
        {
            get { return _woeId; }
            set
            {
                if (_woeId == value)
                {
                    return;
                }

                _woeId = value;
                OnPropertyChanged("WoeId");
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

#if !Smartphone && !NET20
        [DataMember]
#endif
        [JsonProperty("placeType")]
        public virtual WhereOnEarthPlaceType PlaceType
        {
            get { return _placeType; }
            set
            {
                if (_placeType == value)
                {
                    return;
                }

                _placeType = value;
                OnPropertyChanged("PlaceType");
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
        [JsonProperty("countryCode")]
        public virtual string CountryCode
        {
            get { return _countryCode; }
            set
            {
                if (_countryCode == value)
                {
                    return;
                }

                _countryCode = value;
                OnPropertyChanged("CountryCode");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Country
        {
            get { return _country; }
            set
            {
                if (_country == value)
                {
                    return;
                }

                _country = value;
                OnPropertyChanged("Country");
            }
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}