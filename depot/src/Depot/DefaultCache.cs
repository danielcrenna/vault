using System;
using System.Runtime.Caching;

namespace depot
{
    /// <summary>
    /// A cache that uses .NET's built-in memory object cache
    /// </summary>
    public class DefaultCache : ICache
    {
        private readonly ObjectCache _cache = MemoryCache.Default;

        public long Count { get { return _cache.GetCount(); } }

        public bool Set(string key, object value)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy()));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(absoluteExpiration)));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration)));
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(dependency: dependency)));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(absoluteExpiration, dependency: dependency)));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration, dependency: dependency)));
        }

        public bool Add(string key, object value)
        {
            return _cache.Add(key, value, BuildCachePolicy());
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return _cache.Add(key, value, BuildCachePolicy(absoluteExpiration));
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            return _cache.Add(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration));
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            return _cache.Add(key, value, BuildCachePolicy(dependency: dependency));
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _cache.Add(key, value, BuildCachePolicy(absoluteExpiration, dependency: dependency));
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _cache.Add(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration, dependency: dependency));
        }

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, slidingExpiration)));
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, dependency)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, slidingExpiration, dependency)));
        }

        public object Get(string key, Func<object> add, TimeSpan? contentionTimeout = null)
        {
            return this.GetOrAddUntyped(key, add);
        }

        public T Get<T>(string key, Func<T> add, TimeSpan? contentionTimeout = null) where T : class
        {
            return this.GetOrAddTyped(key, add);
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

        private static CacheItemPolicy BuildCachePolicy(
            DateTime? absoluteExpiration = null, 
            TimeSpan? slidingExpiration = null,
            CacheEntryRemovedCallback removed = null, 
            CacheEntryUpdateCallback updated = null,
            ICacheDependency dependency = null)
        {
            var policy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = absoluteExpiration.HasValue ? absoluteExpiration.Value : ObjectCache.InfiniteAbsoluteExpiration,
                SlidingExpiration = slidingExpiration.HasValue ? slidingExpiration.Value : ObjectCache.NoSlidingExpiration,
                RemovedCallback = removed,
                UpdateCallback = updated
            };
            if(dependency != null)
            {
                var fileDependency = dependency as IFileCacheDependency;
                if (fileDependency != null)
                {
                    policy.ChangeMonitors.Add(new HostFileChangeMonitor(fileDependency.FilePaths));       
                }
            }
            return policy;
        }

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