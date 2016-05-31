using System.ComponentModel;

#if Smartphone
using TweetSharp.Core.Attributes;
#endif

namespace TweetSharp
{
    /// <summary>
    /// Represents the possible known date formats that Twitter reports.
    /// </summary>
    public enum TwitterDateFormat
    {
        /// <summary>
        /// RestApi
        /// </summary>
        [Description("ddd MMM dd HH:mm:ss zzzzz yyyy")] RestApi,
        /// <summary>
        /// SearchApi
        /// </summary>
        [Description("ddd, dd MMM yyyy HH:mm:ss zzzzz")] SearchApi,
        /// <summary>
        /// Atom
        /// </summary>
        [Description("yyyy-MM-ddTHH:mm:ssZ")] Atom,
        /// <summary>
        /// XmlHashesAndRss
        /// </summary>
        [Description("yyyy-MM-ddTHH:mm:sszzzzzz")] XmlHashesAndRss,
        /// <summary>
        /// TrendsCurrent
        /// </summary>
        [Description("ddd MMM dd HH:mm:ss zzzzz yyyy")] TrendsCurrent,
        /// <summary>
        /// TrendsDaily
        /// </summary>
        [Description("yyyy-MM-dd HH:mm")] TrendsDaily,
        /// <summary>
        /// TrendsWeekly
        /// </summary>
        [Description("yyyy-MM-dd")] TrendsWeekly
    }
}