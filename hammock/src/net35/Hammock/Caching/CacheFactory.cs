using System;

namespace Hammock.Caching
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public static class CacheFactory
    {
#if !Smartphone && !Silverlight && !ClientProfiles && !MonoTouch && !NETCF
        public static IDependencyCache AspNetCache
        {
            get { return new AspNetCache(); }
        }
#endif

        public static ICache InMemoryCache
        {
            get { return new SimpleCache(); }
        }

#if SILVERLIGHT
        public static ICache IsolatedStorageCache
        {
            get { throw new NotImplementedException(); }
        }
#endif
    }
}