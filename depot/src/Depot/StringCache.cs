using System;
using System.Text;

namespace depot
{
    /// <summary>
    /// A cache decorator that's optimized for storing string content using compression
    /// </summary>
    public class StringCache : CompressionBase, IStringCache
    {
        private readonly ICache _inner;
        private readonly Encoding _encoding;

        public int Threshold { get; set; }

        public StringCache(Encoding encoding = null) : this(new DefaultCache(), encoding)
        {
            
        }
        public StringCache(ICache inner, Encoding encoding = null)
        {
            Threshold = 512;
            _encoding = encoding ?? Encoding.UTF8;
            _inner = inner;
        }

        private string PrepareValue(byte[] data)
        {
            if (data == null) return null;
            var prepared = IsCompressed(data) ? Unzip(data) : data;
            return _encoding.GetString(prepared);
        }
        private byte[] PrepareData(string value)
        {
            if (value == null) return null;
            var prepared = value.Length > Threshold ? Zip(value) : _encoding.GetBytes(value);
            return prepared;
        }

        #region Passthrough

        public bool Set(string key, string value)
        {
            return _inner.Set(key, PrepareData(value));
        }
        
        public bool Set(string key, string value, DateTime absoluteExpiration)
        {
            return _inner.Set(key, PrepareData(value), absoluteExpiration);
        }

        public bool Set(string key, string value, TimeSpan slidingExpiration)
        {
            return _inner.Set(key, PrepareData(value), slidingExpiration);
        }

        public bool Set(string key, string value, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), dependency);
        }

        public bool Set(string key, string value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Set(string key, string value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Set(key, PrepareData(value), slidingExpiration, dependency);
        }

        public bool Add(string key, string value)
        {
            return _inner.Add(key, PrepareData(value));
        }

        public bool Add(string key, string value, DateTime absoluteExpiration)
        {
            return _inner.Add(key, PrepareData(value), absoluteExpiration);
        }

        public bool Add(string key, string value, TimeSpan slidingExpiration)
        {
            return _inner.Add(key, PrepareData(value), slidingExpiration);
        }

        public bool Add(string key, string value, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), dependency);
        }

        public bool Add(string key, string value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Add(string key, string value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Add(key, PrepareData(value), slidingExpiration, dependency);
        }

        public bool Replace(string key, string value)
        {
            return _inner.Replace(key, PrepareData(value));
        }

        public bool Replace(string key, string value, DateTime absoluteExpiration)
        {
            return _inner.Replace(key, PrepareData(value), absoluteExpiration);
        }

        public bool Replace(string key, string value, TimeSpan slidingExpiration)
        {
            return _inner.Replace(key, PrepareData(value), slidingExpiration);
        }

        public bool Replace(string key, string value, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), dependency);
        }

        public bool Replace(string key, string value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), absoluteExpiration, dependency);
        }

        public bool Replace(string key, string value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _inner.Replace(key, PrepareData(value), slidingExpiration, dependency);
        }

        public string Get(string key)
        {
            return PrepareValue(_inner.Get<byte[]>(key));
        }

        public string Get(string key, Func<string> add)
        {
            return PrepareValue(_inner.Get(key, () => PrepareData(add())));
        }

        public string Remove(string key)
        {
            return _inner.Remove<string>(key);
        }

        #endregion
    }
}