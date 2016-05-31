using System;

namespace depot
{
    public interface ICache
    {
        /// <summary>
        /// Adds a new item to the cache if an item with the same key does not exist; if it does exist, it is replaced;
        /// returns <code>true</code> if the item was added successfully and <code>false</code> if it was not
        /// </summary>
        bool Set(string key, object value);
        bool Set(string key, object value, DateTime absoluteExpiration);
        bool Set(string key, object value, TimeSpan slidingExpiration);
        bool Set(string key, object value, ICacheDependency dependency);
        bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Adds a new item to the cache if an item with the same key does not exist;
        /// returns <code>true</code> if the item was added and <code>false</code> if there was an existing key
        /// </summary>
        bool Add(string key, object value);
        bool Add(string key, object value, DateTime absoluteExpiration);
        bool Add(string key, object value, TimeSpan slidingExpiration);
        bool Add(string key, object value, ICacheDependency dependency);
        bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Replaces an existing item in the cache with a new item if an item with the given key exists;
        /// returns <code>true</code> if an item was replaced and <code>false</code> if it did not exist
        /// </summary>
        bool Replace(string key, object value);
        bool Replace(string key, object value, DateTime absoluteExpiration);
        bool Replace(string key, object value, TimeSpan slidingExpiration);
        bool Replace(string key, object value, ICacheDependency dependency);
        bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        /// Retrieves the object from the underlying cache by key.
        /// If there is no object found with that key, null is returned.
        /// </summary>
        object Get(string key);

        /// <summary>
        /// Retrieves the object from the underlying cache by key.
        /// If there is no object found with that key, the type's default value is returned.
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// Retrieves the object from the underlying cache by key.
        /// If there is no object found with that key, the add function is executed and the results are cached and returned.
        /// <remarks>
        /// This is useful to avoid having to write your own checks against the cache in order to insert new objects
        /// </remarks> 
        /// </summary>
        object Get(string key, Func<object> add, TimeSpan? contentionTimeout = null);

        /// <summary>
        /// Retrieves the object from the underlying cache by key.
        /// If there is no object found with that key, the add function is executed and the results are cached and returned.
        /// <remarks>
        /// This is useful to avoid having to write your own checks against the cache in order to insert new objects
        /// </remarks> 
        /// </summary>
        T Get<T>(string key, Func<T> add, TimeSpan? contentionTimeout = null) where T : class;

        /// <summary>
        /// Removes an object by key from the cache. The result of attempting to remove a non-existing item from the cache
        /// is up to the underlying cache implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Remove<T>(string key);
    }
}