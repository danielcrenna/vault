# Sitemaps
## Smart sitemap generation for .NET web apps

### Introduction

    PM> Install-Package Sitemaps

### Features

* Supports static and dynamic sitemap tagging
* Provides browser and client caching mechanisms
* Automatically adheres to the [Sitemaps protocol](http://sitemaps.org)
* Understands ASP.NET MVC, so it obeys areas and action names, resolves method constraints, etc.

### Usage

To configure the service, use the following options in your bootstrapper or `Global.cs` file,
where you would normally register routes:

```csharp
// Register "/sitemap" default route
SitemapService.Register();

// Register with a custom route URL
SitemapService.Register("unicorns");

// Change the default page size (the default is 125)
SitemapService.PageSize = 150;
```

Once configured, you can decorate your action methods with the `Sitemap` attribute.
If you specify a `Sitemap` on a controller rather than an action method, every controller
action method is selected for the sitemap.

```csharp
// Specify a default frequency and priority
[Sitemap]
public ActionResult Team()
{
    return View();
}
    
// Specify a custom frequency
[Sitemap(Frequency = SitemapFrequency.Yearly)]
public ActionResult Privacy()
{
    return View();
}   

// Specify a custom frequency and priority
[Sitemap(Frequency = SitemapFrequency.Hourly, Priority = 1.0]
public ActionResult Blog()
{
    return View();
}
```

The controller action decorations above would produce the following XML sitemap:

```xml
<?xml version="1.0" encoding="utf-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
    <url>
        <loc>http://localhost:9090/Team</loc>
        <lastmod>2011-05-20T04:33:19</lastmod>
        <changefreq>daily</changefreq>
        <priority>0.5</priority>
    </url>
    <url>
        <loc>http://localhost:9090/Privacy</loc>
        <lastmod>2011-05-20T04:33:19</lastmod>
        <changefreq>monthly</changefreq>
        <priority>0.5</priority>
    </url>
    <url>
        <loc>http://localhost:9090/Blog</loc>
        <lastmod>2011-05-20T04:33:19</lastmod>
        <changefreq>hourly</changefreq>
        <priority>1.0</priority>
    </url>
</urlset>
```

If your sitemap contains more URLs than the `SitemapService.PageSize`, it is automatically
broken down into a paginated set of sitemaps per the protocol:
    
```xml
<?xml version="1.0" encoding="utf-8"?>
<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
    <sitemap>
        <loc>http://localhost:9090/sitemap/?page=1</loc>
        <lastmod>2011-05-19T22:57:42</lastmod>
    </sitemap>
    <sitemap>
        <loc>http://localhost:9090/sitemap/?page=2</loc>
        <lastmod>2011-05-19T22:57:42</lastmod>
    </sitemap>
    <sitemap>
        <loc>http://localhost:9090/sitemap/?page=3</loc>
        <lastmod>2011-05-19T22:57:42</lastmod>
    </sitemap>
</sitemapindex>
```

To add your own sitemap URLs at runtime, resolve `ISitemapService` or use `SitemapService`
and call the `AddNode` method, passing in a `SitemapNode` instance:

```csharp
var service = ServiceLocator.Current.GetInstance<ISitemapService>();
service.AddNode(new SitemapNode("http://unicorns.com") { Frequency = Frequency.Always, Priority = 1.0 });
```