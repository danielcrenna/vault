using System.Collections.Generic;

namespace tuxedo.Extensions
{
    internal static class CollectionExtensions
    {
        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right)
        {
            foreach(var item in right)
            {
                left.Add(item);
            }
            return left;
        }
    }
}