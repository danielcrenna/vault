using System;

namespace depot
{
    /// <summary>
    /// A cache that can optimize internal storage of keyed values using compression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResourceCache<T>
    {
        /// <summary>
        /// Adds a new item to the cache if a string with the same key does not exist; if it does exist, it is replaced;
        /// returns <code>true</code> if the string was added successfully and <code>false</code> if it was not
        /// </summary>
        bool Set(string key, T value);
        bool Set(string key, T value, DateTime absoluteExpiration);
        bool Set(string key, T value, TimeSpan slidingExpiration);
        bool Set(string key, T value, ICacheDependency dependency);
        bool Set(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Set(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Adds a new string to the cache if a string with the same key does not exist;
        /// returns <code>true</code> if the string was added and <code>false</code> if there was an existing key
        /// </summary>
        bool Add(string key, T value);
        bool Add(string key, T value, DateTime absoluteExpiration);
        bool Add(string key, T value, TimeSpan slidingExpiration);
        bool Add(string key, T value, ICacheDependency dependency);
        bool Add(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Add(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Replaces an existing string in the cache with a new string if a string with the given key exists;
        /// returns <code>true</code> if a string was replaced and <code>false</code> if it did not exist
        /// </summary>
        bool Replace(string key, T value);
        bool Replace(string key, T value, DateTime absoluteExpiration);
        bool Replace(string key, T value, TimeSpan slidingExpiration);
        bool Replace(string key, T value, ICacheDependency dependency);
        bool Replace(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Replace(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Retrieves the string from the underlying cache by key.
        /// If there is no string found with that key, null is returned.
        /// </summary>
        T Get(string key);

        /// <summary>
        /// Retrieves the string from the underlying cache by key.
        /// If there is no string found with that key, the add function is executed and the results are cached and returned.
        /// <remarks>
        /// This is useful to avoid having to write your own checks against the cache in order to insert new strings
        /// </remarks> 
        /// </summary>
        T Get(string key, Func<T> add);

        /// <summary>
        /// Removes a string by key from the cache. The result of attempting to remove a non-existing string from the cache
        /// is up to the underlying cache implementation.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T Remove(string key);
    }
}