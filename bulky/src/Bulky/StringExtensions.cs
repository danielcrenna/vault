using System.Collections.Generic;
using System.Linq;
using tuxedo;

namespace bulky
{
    internal static class StringExtensions
    {
        public const string DefaultSeparator = ", ";

        public static string Concat(this IEnumerable<object> list, string separator = DefaultSeparator)
        {
            return string.Join(separator, list);
        }
        
        public static string Qualify(this string value, Dialect dialect)
        {
            return string.IsNullOrWhiteSpace(value) ? value : string.Concat(dialect.StartIdentifier, value, dialect.EndIdentifier).Trim();
        }

        public static string ConcatQualified(this IEnumerable<string> list, Dialect dialect, string separator = DefaultSeparator)
        {
            var qualified = list.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Qualify(dialect));
            return string.Join(separator, qualified);
        }
    }
}