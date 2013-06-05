namespace Sitemaps
{
    /// <summary>
    /// Defines the volatility of content tracked via a sitemap node
    /// <seealso href="http://www.volume9inc.com/2009/03/15/sitemap-xml-why-changefreq-priority-are-important/" />
    /// </summary>
    public enum SitemapFrequency
    {
        Never,
        Yearly,
        Monthly,
        Weekly,
        Daily,
        Hourly,
        Always
    }
}