using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TweetSharp
{
    internal class PathHelpers
    {
        public static string ReplaceUriTemplateTokens(List<object> segments, string path)
        {
#if !SILVERLIGHT || WINDOWS_PHONE
            var regexObj = new Regex(@"\{\w*\}", RegexOptions.Compiled);
#else
            var regexObj = new Regex(@"\{\w*\}");
#endif
            var matches = regexObj.Matches(path);
            foreach (Match match in matches)
            {
                var token = match.Value.Substring(1, match.Value.Length - 2);
                path = ReplacePathSegment(path, token, segments);
            }
            return path;
        }

        public static string ReplacePathSegment(string path, string value, List<object> segments)
        {
            var segment = segments.SingleOrDefault(s => s.Equals(string.Format("?{0}=", value)) || s.Equals(string.Format("&{0}=", value)));
            if (segment != null)
            {
                var index = segments.IndexOf(segment);
                path = path.Replace(string.Format("{{{0}}}", value), string.Concat(segments[index + 1]));
                segments.RemoveRange(index, 2);

                // Replace missing ? if the segment was first in the series (after format)
                if(index == 1 && segments.Count > 1)
                {
                    var first = segments[index].ToString();
                    if(first.StartsWith("&"))
                    {
                        segments[index] = string.Concat("?", first.Substring(1));
                    }
                }
            }
            else
            {
                path = path.Replace(string.Format("/{{{0}}}", value), "");
            }
            return path;
        }

        private static readonly Regex _escapeSegments = new Regex(@"\A(?:[?|&]\w*=)\Z"
#if !SILVERLIGHT || WINDOWS_PHONE
            , RegexOptions.Compiled
#endif
            );
        
        public static void EscapeDataContainingUrlSegments(IList<object> segments)
        {
            var names = segments.Where(s => _escapeSegments.IsMatch(s.ToString()));
            var indexes = names.Select(n => segments.IndexOf(n) + 1).ToList();
            for (var i = 0; i < indexes.Count(); i++)
            {
                var segment = segments[indexes[i]];
                var value = segment.ToString();
                segments[indexes[i]] = Uri.EscapeDataString(value);
            }
        }
    }

    internal class StringHelpers
    {
        private const string UnderscoresPattern = "(((?<=[a-z])[A-Z])|([A-Z](?![A-Z]|$)))";

        public static string Capitalize(string upperCase)
        {
            var lower = upperCase.ToLowerInvariant();
            return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
        }
        
        public static string CamelCase(string pascalCase)
        {
            return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
        }

        public static string PascalCase(string camelCase)
        {
            return char.ToUpperInvariant(camelCase[0]) + camelCase.Substring(1);
        }

        public static string Underscore(string camelCase)
        {
            var underscored = Regex.Replace(camelCase, UnderscoresPattern,
                                            new MatchEvaluator(m => string.Concat("_", m.Value.ToLowerInvariant()))
#if !SILVERLIGHT
                                            ,RegexOptions.Compiled
#endif
                                            );

            return underscored;
        }
    }
}
