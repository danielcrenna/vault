using System;
using System.Collections.Generic;
using System.Web;

#if !ClientProfiles
using System.Web.Caching;
#endif

namespace Hammock.Caching
{
#if !ClientProfiles
#if !SILVERLIGHT
    [Serializable]
#endif
    public class AspNetCache : IDependencyCache
    {
        #region IDependencyCache Members

        public virtual int Count
        {
            get { return HttpRuntime.Cache.Count; }
        }

        public virtual void Add(string key, object value, CacheDependency dependency, DateTime absoluteExpiration,
                        TimeSpan slidingExpiration, CacheItemPriority priority,
                        CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Add(key, value, dependency, absoluteExpiration, Cache.NoSlidingExpiration, priority,
                                  onRemoveCallback);
        }

        public virtual void Insert(string key, object value)
        {
            HttpRuntime.Cache.Insert(key, value);
        }

        public virtual void Insert(string key, object value, DateTime absoluteExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public virtual void Insert(string key, object value, TimeSpan slidingExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                           CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration, priority,
                                     onRemoveCallback);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration, priority,
                                     onRemoveCallback);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                           CacheItemUpdateCallback onUpdateCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration,
                                     onUpdateCallback);
        }

        public virtual void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           CacheItemUpdateCallback onUpdateCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration,
                                     onUpdateCallback);
        }

        public virtual void Clear()
        {
            var keys = new List<string>();
            var enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                keys.Add(key);
            }

            foreach (var key in keys)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        public virtual T Get<T>(string key)
        {
            return (T) HttpRuntime.Cache.Get(key);
        }

        public virtual void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public virtual IEnumerable<string> Keys
        {
            get
            {
                var enumerator = HttpRuntime.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Key.ToString();
                }
            }
        }

        #endregion
    }
#endif
}