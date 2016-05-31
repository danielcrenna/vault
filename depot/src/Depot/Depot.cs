using System;
using container;

namespace depot
{
    /// <summary>
    /// An access point for pre-configured caches and configuration settings
    /// </summary>
    public static class Depot
    {
        private const string ObjectCacheKey = "__Depot__ObjectCache";
        private const string ContentCacheKey = "__Depot__ContentCache";
        private const string StringCacheKey = "__Depot__StringCache";
        
        public static Container Container { get; private set; }
        static Depot()
        {
            Container = new Container();
            Container.Register<ICache>(ObjectCacheKey, r => new DefaultCache()).Permanent();
            Container.Register<IStringCache>(StringCacheKey, r => new StringCache()).Permanent();
            Container.Register<IContentCache>(ContentCacheKey, r => new ContentCache()).Permanent();
            ContentionTimeout = TimeSpan.FromSeconds(10);
        }

        public static TimeSpan ContentionTimeout { get; set; }
        
        public static ICache ObjectCache
        {
            get { return Container.Resolve<ICache>(ObjectCacheKey); }
        }

        public static IContentCache ContentCache
        {
            get
            {
                return Container.Resolve<IContentCache>(ContentCacheKey);
            }
        }

        public static IStringCache StringCache
        {
            get
            {
                return Container.Resolve<IStringCache>(StringCacheKey);
            }
        }
    }
}