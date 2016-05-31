using System;
using System.Collections.Generic;

namespace depot
{
    internal static class CacheAccessors
    {
        public static object GetOrAddUntyped(this ICache cache, string key, Func<object> add, TimeSpan? timeout = null)
        {
            var item = cache.Get(key);
            if (item == null)
            {
                if (add == null)
                {
                    return null;
                }
                using (TimedLock.Lock(CacheLockScope.AcquireLock<object>(key), timeout ?? Depot.ContentionTimeout))
                {
                    var itemToAdd = add();
                    if (itemToAdd != null)
                    {
                        cache.Add(key, itemToAdd);
                    }
                }
                return cache.Get(key);
            }
            return item;
        }

        public static T GetOrAddTyped<T>(this ICache cache, string key, Func<T> add, TimeSpan? timeout = null) where T : class
        {
            var item = cache.Get(key);
            if (item == null)
            {
                if (add == null)
                {
                    return default(T);
                }
                using (TimedLock.Lock(CacheLockScope.AcquireLock<T>(key), timeout ?? Depot.ContentionTimeout))
                {
                    var itemToAdd = add();
                    if(itemToAdd != null) 
                    {
                        cache.Add(key, itemToAdd);
                    }
                }
                return cache.Get<T>(key);
            }
            return (T)item;
        }

        private static class CacheLockScope
        {
            private static readonly Dictionary<Type, IDictionary<string, object>> Locks;
            static CacheLockScope()
            {
                Locks = new Dictionary<Type, IDictionary<string, object>>();
            }
            public static object AcquireLock<T>(string key)
            {
                IDictionary<string, object> hash;
                if (!Locks.TryGetValue(typeof(T), out hash))
                {
                    hash = new Dictionary<string, object>();
                    Locks.Add(typeof(T), hash);
                }
                object @lock;
                if (!hash.TryGetValue(key, out @lock))
                {
                    @lock = new object();
                    hash.Add(key, @lock);
                }
                return @lock;
            }
        }
    }
}