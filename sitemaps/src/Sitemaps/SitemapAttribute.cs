using System;
using System.Web.Mvc;

namespace Sitemaps
{
    /// <summary>
    /// A filter attribute for specifying that routes that map to a controller or action should be indexed in the sitemap
    /// </summary>
    public class SitemapAttribute : ActionFilterAttribute
    {
        public SitemapFrequency Frequency { get; set; }
        public double Priority { get; set; }
        public DateTime LastModified { get; set; }
        
        public SitemapAttribute()
        {
            Frequency = SitemapFrequency.Daily;
            Priority = 0.5;
        }
    }
}
