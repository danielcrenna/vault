using System;

namespace depot
{
    /// <summary>
    /// A cache decorator that's optimized for storing binary content using compression
    /// </summary>
    public class ContentCache : CompressionBase, IContentCache
    {
        private readonly ICache _inner;
        public int Threshold { get; set; }

        public ContentCache() : this(new DefaultCache())
        {
            
        }
        public ContentCache(ICache inner)
        {
            Threshold = 512;
            _inner = inner;
        }

        private static byte[] PrepareValue(byte[] data)
        {
            if (data == null) return null;
            return IsCompressed(data) ? Unzip(data) : data;
        }
        private byte[] PrepareData(byte[] value)
        {
            if (value == null) return null;
            return value.Length > Threshold ? Zip(value) : value;
        }

        #region Passthrough

        public bool Set(string key, byte[] value)
        {
            return _inner.Set(key, PrepareData(value));
        }

        public bool Set(string key, byte[] value, DateTime absoluteExpiration)
        {
            return _inner.Set(key, PrepareData(value), absoluteExpiration);
        }

        public bool Set(string key, byte[] value, TimeSpan slidingExpiration)
        {
            return _inner.Set(key, PrepareData(value), slidingExpiration);
        }

        public bool Set(string key, byte[] value, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), dependency);
        }

        public bool Set(string key, byte[] value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Set(string key, byte[] value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), slidingExpiration, dependency);
        }

        public bool Add(string key, byte[] value)
        {
            return _inner.Add(key, PrepareData(value));
        }

        public bool Add(string key, byte[] value, DateTime absoluteExpiration)
        {
            return _inner.Add(key, PrepareData(value), absoluteExpiration);
        }

        public bool Add(string key, byte[] value, TimeSpan slidingExpiration)
        {
            return _inner.Add(key, PrepareData(value), slidingExpiration);
        }

        public bool Add(string key, byte[] value, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), dependency);
        }

        public bool Add(string key, byte[] value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Add(string key, byte[] value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), slidingExpiration, dependency);
        }

        public bool Replace(string key, byte[] value)
        {
            return _inner.Replace(key, PrepareData(value));
        }

        public bool Replace(string key, byte[] value, DateTime absoluteExpiration)
        {
            return _inner.Replace(key, PrepareData(value), absoluteExpiration);
        }

        public bool Replace(string key, byte[] value, TimeSpan slidingExpiration)
        {
            return _inner.Replace(key, PrepareData(value), slidingExpiration);
        }

        public bool Replace(string key, byte[] value, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), dependency);
        }

        public bool Replace(string key, byte[] value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Replace(string key, byte[] value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), slidingExpiration, dependency);
        }

        public byte[] Get(string key)
        {
            return PrepareValue(_inner.Get<byte[]>(key));
        }

        public byte[] Get(string key, Func<byte[]> add)
        {
            return PrepareValue(_inner.Get(key, () => PrepareData(add())));
        }

        public byte[] Remove(string key)
        {
            return _inner.Remove<byte[]>(key);
        }

        #endregion
    }
}