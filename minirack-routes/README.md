# minirack-routing
## Provides zero-configuration canonical route support 

```
PM> Install-Package minirack-routes
```

This is a small single purpose self-installing module that provides canonical routing features for SEO.
It does so without requiring changes to your existing routing, so you can enable it as required, in 
certain environments or in production vs. debugging, etc.

### Features
This module provides the following options, all on by default:
* Canonical routing - permanently redirects all incoming requests to lowercase, if they are not already
* Canonical routing - permanently redirects URLs with trailing slashes, to the equivalent URL without it
* Lowercase routing - can optionally rewrite all existing ASP.NET routes to lowercase. This is useful for
avoiding the overhead of redirecting all internal routes that may not be in lowercase already, either by
omission or preference.

### Usage

```csharp
public class MyApplication : HttpApplication
{
    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);

        // These are the default settings
        Routing.CanonicalRemoveTrailingSlash = true;
        Routing.CanonicalLowercase = true;
        Routing.LowercaseRoutes = true;     
		
		// Add any routes (by routing URL) you want to ignore
		Routing.IgnoreRoutes.Add("/Content/{*pathInfo}");   
        
        // Call this after all other routes are registered
        Routing.Initialize();
    }
}
```
