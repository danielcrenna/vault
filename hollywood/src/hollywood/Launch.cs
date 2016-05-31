using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace hollywood
{
    public class Launch
    {
        internal static readonly Lazy<List<string>> AllowedUrls = new Lazy<List<string>>(() => new List<string>()); 
        private static readonly Lazy<LaunchFilter> LaunchFilter = new Lazy<LaunchFilter>(() => new LaunchFilter());

        public static void InstallPrelaunch()
        {
            GlobalFilters.Filters.Add(LaunchFilter.Value);
            AtTopOfRouteTable(routes => routes.MapRoute(
                name: "Basis_Prelaunch",
                url: "prelaunch",
                defaults: new { controller = "Prelaunch", action = "Index" },
                namespaces: new[] { "hollywood" }
            ));
        }

        public void AddAllowedUrls(params string[] url)
        {
            AllowedUrls.Value.AddRange(url);
        }

        public void AddAllowedUrls(IEnumerable<string> urls)
        {
            AllowedUrls.Value.AddRange(urls);
        }

        public void AddAllowedUrl(string url)
        {
            AllowedUrls.Value.Add(url);
        }

        private static void AtTopOfRouteTable(Action<RouteCollection> script)
        {
            var currentRoutes = new List<RouteBase>(RouteTable.Routes.Count);
            currentRoutes.AddRange(RouteTable.Routes);
            RouteTable.Routes.Clear();
            var routes = RouteTable.Routes;
            script(routes);
            foreach (var route in currentRoutes)
            {
                RouteTable.Routes.Add(route);
            }
        }
    }
}
