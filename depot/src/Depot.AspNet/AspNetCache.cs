using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace depot.AspNet
{
    /// <summary>
    /// Uses the ASP.NET Cache to cache objects in in-process memory.
    /// </summary>
    public class AspNetCache : ICache
    {
        private readonly object _sync = new object();
        public object Sync { get { return _sync; } }

        private readonly Cache _cache = HttpRuntime.Cache;

        private bool Insert(string key, object value)
        {
            return Try(() => _cache.Insert(key, value));
        }

        private bool Insert(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() => _cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration));
        }

        private bool Insert(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() => _cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration));
        }

        private bool Insert(string key, object value, ICacheDependency dependency)
        {
            var aspNetDependency = ((AspNetFileCacheDependency)dependency).Internal;
            return Try(() => _cache.Insert(key, value, aspNetDependency));
        }

        private bool Insert(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            var aspNetDependency = ((AspNetFileCacheDependency)dependency).Internal;
            return Try(() => _cache.Insert(key, value, aspNetDependency, absoluteExpiration, Cache.NoSlidingExpiration));
        }

        private bool Insert(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            var aspNetDependency = ((AspNetFileCacheDependency)dependency).Internal;
            return Try(() => _cache.Insert(key, value, aspNetDependency, Cache.NoAbsoluteExpiration, slidingExpiration));
        }

        public bool Set(string key, object value)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration));
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, dependency));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration, dependency));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration, dependency));
        }

        public bool Add(string key, object value)
        {
            return _cache.Get(key) == null && Insert(key, value);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return _cache.Get(key) == null && Insert(key, value, absoluteExpiration);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            return _cache.Get(key) == null && Insert(key, value, slidingExpiration);
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, dependency);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, absoluteExpiration, dependency);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, slidingExpiration, dependency);
        }

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value, slidingExpiration)));
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value, dependency)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Insert(key, value, slidingExpiration, dependency)));
        }

        public object Get(string key, Func<object> add, TimeSpan? contentionTimeout = null)
        {
            return this.GetOrAddUntyped(key, add, contentionTimeout);
        }

        public T Get<T>(string key, Func<T> add, TimeSpan? contentionTimeout = null) where T : class
        {
            return this.GetOrAddTyped(key, add, contentionTimeout);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public T Get<T>(string key)
        {
            var item = _cache.Get(key);
            if (item == null)
            {
                return default(T);
            }
            return (T)item;
        }
        
        public T Remove<T>(string key)
        {
            return (T)_cache.Remove(key);
        }

        public void Clear()
        {
            var keys = new List<string>();

            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                keys.Add(key);
            }

            foreach (var t in keys)
            {
                _cache.Remove(t);
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                var enumerator = _cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Key.ToString();
                }
            }
        }

        public int Count { get { return _cache.Count; } }

        private static bool Try(Action closure)
        {
            try
            {
                closure();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveIfKeyExistsAndThen(string key, Func<bool> operation)
        {
            try
            {
                if (_cache[key] != null)
                {
                    _cache.Remove(key);
                }
                return operation();
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveKeyAndThen(string key, Func<bool> operation)
        {
            try
            {
                _cache.Remove(key);
                return operation();
            }
            catch
            {
                return false;
            }
        }

        private bool EnsureKeyExistsAndThen(string key, Func<bool> operation)
        {
            try
            {
                return _cache[key] != null && operation();
            }
            catch
            {
                return false;
            }
        }
    }
}
