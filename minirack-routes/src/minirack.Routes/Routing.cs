using System.Collections.Generic;
using System.Web.Routing;

namespace minirack.Routes
{
    public static class Routing
    {
        static Routing()
        {
            CanonicalLowercase = true;
            CanonicalRemoveTrailingSlash = true;
            LowercaseRoutes = true;
            IgnoreRoutes = new HashSet<string>();
        }

        public static bool CanonicalRemoveTrailingSlash { get; set; }
        public static bool CanonicalLowercase { get; set; }
        public static bool LowercaseRoutes { get; set; }
        public static ICollection<string> IgnoreRoutes { get; set; }

        public static void Initialize()
        {
            if (!LowercaseRoutes)
            {
                return;
            }
            using (RouteTable.Routes.GetWriteLock())
            {
                var routes = RouteTable.Routes;
                for (var i = 0; i < routes.Count; i++)
                {
                    Route route = routes[i] as Route;
                    if(route != null && IgnoreRoutes.Contains(route.Url))
                    {
                        continue;
                    }
                    routes[i] = new LowercaseDecorator(route);
                }
            }
        }
    }
}

    