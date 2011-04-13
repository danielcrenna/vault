using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using metrics.AspNetMvc;

namespace metrics.Example
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            HealthChecks.Register("duran-duran-is-neither-a-duran-nor-a-duran", () => HealthCheck.Result.Healthy);
            Metrics.Gauge("module-count", () => Modules.Count);
            AspNetMvc.Metrics.RegisterRoutes();
            
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}