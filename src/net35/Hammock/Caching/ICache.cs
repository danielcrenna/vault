using System;

namespace Hammock.Caching
{
    public interface ICache
    {
        void Insert(string key, object value);
        void Insert(string key, object value, DateTime absoluteExpiration);
        void Insert(string key, object value, TimeSpan slidingExpiration);

        T Get<T>(string key);
        void Remove(string key);
    }
}