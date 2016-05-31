using System;

namespace depot.Mvc
{
    /// <summary>
    /// We need this class to get around the problem of the default model binder not knowing which concrete class to use
    /// </summary>
    public class DepotCache : ICache
    {
        private readonly ICache _inner;

        public DepotCache()
        {
            _inner = Depot.ObjectCache;
        }

        public bool Set(string key, object value)
        {
            return _inner.Set(key, value);
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return _inner.Set(key, value, absoluteExpiration);
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return _inner.Set(key, value, slidingExpiration);
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return _inner.Set(key, value, dependency);
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, value, absoluteExpiration, dependency);
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, value, slidingExpiration, dependency);
        }

        public bool Add(string key, object value)
        {
            return _inner.Add(key, value);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return _inner.Add(key, value, absoluteExpiration);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            return _inner.Add(key, value, slidingExpiration);
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            return _inner.Add(key, value, dependency);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, value, absoluteExpiration, dependency);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, value, slidingExpiration, dependency);
        }

        public bool Replace(string key, object value)
        {
            return _inner.Replace(key, value);
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return _inner.Replace(key, value, absoluteExpiration);
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return _inner.Replace(key, value, slidingExpiration);
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return _inner.Replace(key, value, dependency);
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, value, absoluteExpiration, dependency);
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, value, slidingExpiration, dependency);
        }

        public object Get(string key)
        {
            return _inner.Get(key);
        }

        public T Get<T>(string key)
        {
            return _inner.Get<T>(key);
        }

        public object Get(string key, Func<object> add, TimeSpan? contentionTimeout = null)
        {
            return _inner.Get(key, add, contentionTimeout);
        }

        public T Get<T>(string key, Func<T> add, TimeSpan? contentionTimeout = null) where T : class
        {
            return _inner.Get<T>(key, add, contentionTimeout);
        }

        public T Remove<T>(string key)
        {
            return _inner.Remove<T>(key);
        }
    }
}