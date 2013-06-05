using System;

namespace Sitemaps
{
    public class SitemapNode
    {
        public string Url { get; set; }
        public DateTime LastModified { get; set; }
        public SitemapFrequency Frequency { get; set; }
        public double Priority { get; set; }

        public SitemapNode(string url)
        {
            Url = url;
            Priority = 0.5;
            Frequency = SitemapFrequency.Daily;
            LastModified = DateTime.UtcNow;
        }
    }
}