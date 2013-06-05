using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TweetSharp
{
    internal static class StringExtensions
    {
        public static bool IsNullOrBlank(this string input)
        {
            return string.IsNullOrEmpty(input) || input.Trim().Length == 0;
        }

        public static bool AreNullOrBlank(this IEnumerable<string> values)
        {
            if (!values.Any() || values == null)
            {
                return false;
            }

            return values.Aggregate(true, (current, value) => current & value.IsNullOrBlank());
        }

        public static TwitterEntities ParseTwitterageToEntities(this string text)
        {
            var entities = new TwitterEntities
                               {
                                   HashTags = new List<TwitterHashTag>(ParseTwitterageToHashTags(text)),
                                   Mentions = new List<TwitterMention>(ParseTwitterageToMentions(text)),
                                   Urls = new List<TwitterUrl>(ParseTwitterageToUrls(text))
                               };

            return entities;
        }

        private const RegexOptions Options =
#if !SILVERLIGHT
 RegexOptions.Compiled | RegexOptions.IgnoreCase;
#else
            RegexOptions.IgnoreCase;
#endif

        // Jon Gruber's URL Regex: http://daringfireball.net/2009/11/liberal_regex_for_matching_urls
        private static readonly Regex ParseUrls =
            new Regex(@"\b(([\w-]+://?|www[.])[^\s()<>]+(?:\([\w\d]+\)|([^\p{P}\s]|/)))", Options);

        // Diego Sevilla's @ Regex: http://stackoverflow.com/questions/529965/how-could-i-combine-these-regex-rules
        private static readonly Regex ParseMentions = new Regex(@"(^|\W)@([A-Za-z0-9_]+)", Options);

        // Simon Whatley's # Regex: http://www.simonwhatley.co.uk/parsing-twitter-usernames-hashtags-and-urls-with-javascript
        private static readonly Regex ParseHashtags = new Regex("[#]+[A-Za-z0-9-_]+", Options);

        public static string ParseTwitterageToHtml(this string input)
        {
            if (input.IsNullOrBlank())
            {
                return input;
            }

            foreach (Match match in ParseUrls.Matches(input))
            {
                input = input.Replace(match.Value, string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" target=\"_blank\">{0}</a>", match.Value));
            }

            foreach (Match match in ParseMentions.Matches(input))
            {
                if (match.Groups.Count != 3)
                {
                    continue;
                }

                var screenName = match.Groups[2].Value;
                var mention = "@" + screenName;

                input = input.Replace(mention, string.Format(CultureInfo.InvariantCulture, "<a href=\"https://twitter.com/{0}\" target=\"_blank\">{1}</a>", screenName, mention));
            }

            foreach (Match match in ParseHashtags.Matches(input))
            {
                var hashtag = Uri.EscapeDataString(match.Value);
                input = input.Replace(match.Value, string.Format(CultureInfo.InvariantCulture, "<a href=\"https://twitter.com/search?q={0}\" target=\"_blank\">{1}</a>", hashtag, match.Value));
            }

            return input;
        }

        public static IEnumerable<TwitterUrl> ParseTwitterageToUrls(this string input)
        {
            if (input.IsNullOrBlank())
            {
                yield break;
            }

            foreach (Match match in ParseUrls.Matches(input))
            {
                var value = match.Value;
                
                Uri uri;
                try
                {
                    uri = new Uri(value);
                }
                catch (UriFormatException)
                {
                    continue;
                }

                var url = new TwitterUrl
                              {
                                  Value = uri.ToString(),
                                  Indices = new List<int>(new[] { match.Index, match.Index + match.Value.Length }) 
                              };

                if(!match.Value.EndsWith("/") && url.Value.EndsWith("/"))
                {
                    url.Value = url.Value.Substring(0, url.Value.Length - 1);
                }
                
                yield return url;
            }
        }

        public static IEnumerable<TwitterMention> ParseTwitterageToMentions(this string input)
        {
            if (input.IsNullOrBlank())
            {
                yield break;
            }

            foreach (Match match in ParseMentions.Matches(input))
            {
                if (match.Groups.Count != 3)
                {
                    continue;
                }

                var screenName = match.Groups[2].Value;
                var startIndex = match.Index + (match.Index == 0 ? 0 : 1);
                var mention = new TwitterMention
                                  {
                                      ScreenName = screenName,
                                      Indices = new[] {startIndex, startIndex + screenName.Length + 1}
                                  };

                yield return mention;
            }
        }

        public static IEnumerable<TwitterHashTag> ParseTwitterageToHashTags(this string input)
        {
            if (input.IsNullOrBlank())
            {
                yield break;
            }

            foreach (Match match in ParseHashtags.Matches(input))
            {
                var hashtag = new TwitterHashTag
                                  {
                                      Text = match.Value.Substring(1),
                                      Indices = new[] {match.Index, match.Index + match.Value.Length}
                                  };

                yield return hashtag;
            }
        }

        public static string LegacyTextAsHtml(this ITweetable tweetable)
        {
            return !string.IsNullOrEmpty(tweetable.Text) ? tweetable.Text.ParseTwitterageToHtml() : tweetable.Text;
        }

        public static string ParseTextWithEntities(this ITweetable tweetable)
        {
            if ((tweetable.Entities == null) || !tweetable.Entities.Any())
            {
                return tweetable.LegacyTextAsHtml();
            }
            var builder = new StringBuilder(tweetable.Text);
            var list = new List<TextChange>();
            foreach (var entity in tweetable.Entities)
            {
                var startIndex = entity.StartIndex;
                var num2 = entity.EndIndex - startIndex;
                var mention = entity as TwitterMention;
                string value;
                if (mention != null)
                {
                   value = string.Format(CultureInfo.InvariantCulture, "<a href=\"https://twitter.com/{0}\" target=\"_blank\">@{0}</a>", new object[] { mention.ScreenName });
                   list.Add(new TextChange { Start = startIndex, Length = num2, Value = value });
                }
                var tag = entity as TwitterHashTag;
                if (tag != null)
                {
                    value = string.Format(CultureInfo.InvariantCulture, "<a href=\"https://twitter.com/search?q={0}\" target=\"_blank\">#{1}</a>", new object[] { Uri.EscapeDataString(tag.Text), tag.Text });
                    list.Add(new TextChange { Start = startIndex, Length = num2, Value = value });
                }
                var url = entity as TwitterUrl;
                if (url != null)
                {
                    value = string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" target=\"_blank\">{1}</a>", new object[] { url.ExpandedValue, url.Value });
                    list.Add(new TextChange { Start = startIndex, Length = num2, Value = value });
                }
                var media = entity as TwitterMedia;
                if (media == null) continue;
                value = string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" target=\"_blank\">{1}</a>", new object[] { media.ExpandedUrl, media.DisplayUrl });
                list.Add(new TextChange { Start = startIndex, Length = num2, Value = value });
            }
            for (var i = 0; i < list.Count; i++)
            {
                var change5 = list[i];
                builder.Remove(change5.Start, change5.Length);
                builder.Insert(change5.Start, change5.Value);
                var num4 = change5.Value.Length - change5.Length;
                if ((num4 <= change5.Length) || ((i + 1) >= list.Count)) continue;
                for (var j = i + 1; j < list.Count; j++)
                {
                    var change6 = list[j];
                    change6.Start += num4;
                }
            }
            return builder.ToString();
        }

        internal class TextChange
        {
            public int Length { get; set; }
            public int Start { get; set; }
            public string Value { get; set; }
        }
    }
}
