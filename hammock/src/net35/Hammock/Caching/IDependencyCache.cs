#if !ClientProfiles
using System;
using System.Web.Caching;

namespace Hammock.Caching
{
    public interface IDependencyCache : ICache
    {
        void Add(string key, object value, CacheDependency dependency, DateTime absoluteExpiration,
                 TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback removedCallback);
       
        void Insert(string key, object value, CacheDependency dependencies);
        void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration);
        void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration);

#if !Mono
        void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                    CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback);

        void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                    CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback);

        void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                    CacheItemUpdateCallback onUpdateCallback);

        void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                    CacheItemUpdateCallback onUpdateCallback);
#endif
        void Clear();
    }
}
#endif