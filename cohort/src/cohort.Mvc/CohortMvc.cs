using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using cohort.Models;
using cohort.Mvc.Configuration;

namespace cohort.Mvc
{
    public class CohortMvc
    {
        private const string CohortControllerNamespace = "cohort.Controllers";

        public static void Register()
        {
            ModelBinders.Binders.DefaultBinder = new CohortModelBinder();
            MapRoutes();

            // Any dependencies we don't register are provided by the default resolver 
            var fallbackResolver = DependencyResolver.Current;
            DependencyResolver.SetResolver(new CohortDependencyResolver(Cohort.Container, fallbackResolver));
            
            //BillingConfiguration.SynchronizePlans();
        }

        private static void MapRoutes()
        {
            var repository = Cohort.Container.Resolve<IConfigRepository>();
            var all = repository.GetAll().ToDictionary(s => s.Key, s => s.Value);

            using (RouteTable.Routes.GetReadLock())
            {
                AtTopOfRouteTable(routes =>
                {
                    bool pagespeed;
                    if (bool.TryParse(all["cohort.pagespeed.enabled"], out pagespeed) && pagespeed)
                    {
                        // Intercept files in asset directories for optimization
                        routes.RouteExistingFiles = true;
                        routes.MapRoute(
                            name: "Cohort_Scripts_Folder",
                            url: "Scripts/{*pathInfo}",
                            defaults: new { controller = "File", action = "Serve" },
                            namespaces: new[] { CohortControllerNamespace }
                            );
                        routes.MapRoute(
                            name: "Cohort_Content_Folder",
                            url: "Content/{*pathInfo}",
                            defaults: new { controller = "File", action = "Serve" },
                            namespaces: new[] { CohortControllerNamespace }
                            );
                    }
                    // Match to configuration routes
                    routes.MapRoute(
                        name: "Cohort_SignIn",
                        url: all["cohort.routing.signin"],
                        defaults: new { controller = "Cohort", action = "SignIn" },
                        namespaces: new[] { CohortControllerNamespace }
                        );
                    routes.MapRoute(
                        name: "Cohort_SignUp",
                        url: all["cohort.routing.signup"],
                        defaults: new { controller = "Cohort", action = "SignUp" },
                        namespaces: new[] { CohortControllerNamespace }
                        );
                    routes.MapRoute(
                        name: "Cohort_404",
                        url: all["cohort.routing.404"],
                        defaults: new { controller = "Error", action = "NotFound" },
                        namespaces: new[] { CohortControllerNamespace }
                        );
                    routes.MapRoute(
                        name: "Cohort_500",
                        url: all["cohort.routing.500"],
                        defaults: new { controller = "Error", action = "Unknown" },
                        namespaces: new[] { CohortControllerNamespace }
                        );
                });
            }
        }

        internal static void AtTopOfRouteTable(Action<RouteCollection> script)
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
