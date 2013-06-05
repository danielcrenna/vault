using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Hammock.Model;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// Represents a user's saved search query, for convenient re-querying of the Search API.
    /// </summary>
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
    [DebuggerDisplay("{Name}:'{Query}'")]
#endif
    [JsonObject(MemberSerialization.OptIn)]
    public class TwitterSavedSearch : PropertyChangedBase,
                                      IComparable<TwitterSavedSearch>,
                                      IEquatable<TwitterSavedSearch>,
                                      ITwitterModel
    {
        private long _id;
        private string _name;
        private string _query;
        private string _position;
        private DateTime _createdDate;

        /// <summary>
        /// Gets the ID of this saved search.
        /// </summary>
        /// <value>The saved search ID.</value>
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

        /// <summary>
        /// Gets or sets the user-provided name for this saved search.
        /// </summary>
        /// <value>The saved search name.</value>
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

        /// <summary>
        /// Gets or sets the Search API query used for this saved search.
        /// </summary>
        /// <value>The search query.</value>
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Query
        {
            get { return _query; }
            set
            {
                if (_query == value)
                {
                    return;
                }

                _query = value;
                OnPropertyChanged("Query");
            }
        }

        /// <summary>
        /// Gets or sets the saved search's position in the user's list of saved searches.
        /// </summary>
        /// <value>The saved search position.</value>
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string Position
        {
            get { return _position; }
            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                OnPropertyChanged("Position");
            }
        }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        /// <value>The created date.</value>
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
        public virtual string RawSource { get; set; }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(TwitterSavedSearch other)
        {
            return other.Id == Id ? 0 : other.Id > Id ? -1 : 1;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="savedSearch">The saved search.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="savedSearch"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TwitterSavedSearch savedSearch)
        {
            if (ReferenceEquals(null, savedSearch))
            {
                return false;
            }
            if (ReferenceEquals(this, savedSearch))
            {
                return true;
            }
            return savedSearch.Id == Id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="status">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="status"/> parameter is null.</exception>
        public override bool Equals(object status)
        {
            if (ReferenceEquals(null, status))
            {
                return false;
            }
            if (ReferenceEquals(this, status))
            {
                return true;
            }
            return status.GetType() == typeof(TwitterSavedSearch) &&
                   Equals((TwitterSavedSearch)status);
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
        public static bool operator ==(TwitterSavedSearch left, TwitterSavedSearch right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TwitterSavedSearch left, TwitterSavedSearch right)
        {
            return !Equals(left, right);
        }
    }
}