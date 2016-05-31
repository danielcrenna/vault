using System;
using System.Collections.Generic;

namespace Hammock.Caching
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class SimpleCache : ICache
    {
        private const string NotSupportedMessage = "This simple cache does not support expiration.";

        private static readonly IDictionary<string, object> _cache = new Dictionary<string, object>(0);

        public virtual int Count
        {
            get { return _cache.Count; }
        }

        public virtual IEnumerable<string> Keys
        {
            get { return _cache.Keys; }
        }

        #region ICache Members

        public virtual void Insert(string key, object value)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, value);
            }
            else
            {
                _cache[key] = value;
            }
        }

        public virtual void Insert(string key, object value, DateTime absoluteExpiration)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        public virtual void Insert(string key, object value, TimeSpan slidingExpiration)
        {
            throw new NotSupportedException(NotSupportedMessage);
        }

        public virtual T Get<T>(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return (T)_cache[key];
            }
            return default(T);
        }

        public virtual void Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }

        #endregion
    }
}
