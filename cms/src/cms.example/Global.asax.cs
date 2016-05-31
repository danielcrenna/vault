using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using cms.Models;
using container;

namespace cms.example
{
    public class MvcApplication : HttpApplication
    {
        private static Container _container;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var path = HostingEnvironment.MapPath("~/App_Data");
            _container = new Container();
            _container.Register<IPostRepository>(r => new FileBasedPostRepository(path));
            DependencyResolver.SetResolver(new CmsResolver(_container, DependencyResolver.Current));
        }
    }
}