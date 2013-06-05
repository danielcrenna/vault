using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// A generic collection that also contains any cursor data necessary for paging
    /// using Twitter's cursor feature.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
#endif
    public class TwitterCursorList<T> : List<T>, ICursored
    {
#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual long? NextCursor { get; set; }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual long? PreviousCursor { get; set; }

        public TwitterCursorList(IEnumerable<T> collection) : base(collection)
        {

        }

        public TwitterCursorList(IEnumerable collection)
        {
            foreach(var item in collection)
            {
                Add((T)item);
            }
        }
    }
}