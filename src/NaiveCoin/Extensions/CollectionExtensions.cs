using System.Collections.Generic;

namespace NaiveCoin.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items, int size = 100)
        {
            var count = 0;
            var chunk = new List<T>(size);
            foreach (var element in items)
            {
                if (count++ == size)
                {
                    yield return chunk;
                    chunk = new List<T>(size);
                    count = 1;
                }
                chunk.Add(element);
            }
            yield return chunk;
        }
    }
}