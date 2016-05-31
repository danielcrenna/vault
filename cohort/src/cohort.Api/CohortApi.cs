using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;
using cohort.API.Filters;
using cohort.API.Formatters;
using cohort.API.Handlers;
using cohort.API.Streaming;
using cohort.Api.Authentication;
using cohort.Api.Configuration;
using cohort.Models;
using cohort.Services;
using HttpContextShim;

namespace cohort.API
{
    public static class CohortApi
    {
        public static void Register(HttpConfiguration config)
        {
            ConfigureDefaultAuthenticationProviders();
            ConfigureDefaultFormatters(config);
            ConfigureDefaultHandlers(config);

            // Any dependencies we don't register are provided by the default resolver 
            var fallbackResolver = GlobalConfiguration.Configuration.DependencyResolver;
            GlobalConfiguration.Configuration.DependencyResolver = new CohortDependencyResolver(Cohort.Container, fallbackResolver);

            // For override exception behavior to capture errors
            GlobalConfiguration.Configuration.Filters.Add(new ErrorFilterAttribute());

            // For efficient HTTP file uploads
            config.Services.Replace(typeof(IHostBufferPolicySelector), new NoBufferPolicySelector());
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
        }

        private static void ConfigureDefaultAuthenticationProviders()
        {
            Cohort.Container.Register<IAuthenticationProvider>(
                "BasicAuthenticationProvider",
                r => new BasicAuthenticationProvider(r.Resolve<AccountService>())
            );
            Cohort.Container.Register<IAuthenticationProvider>(
                "TokenAuthenticationProvider",
                r => new TokenAuthenticationProvider(r.Resolve<ITokenRepository>())
            );
        }

        private static void ConfigureDefaultHandlers(HttpConfiguration configuration)//, Uri baseAddress)
        {
            configuration.MessageHandlers.Clear();
            configuration.MessageHandlers.Add(new HttpContextHandler());
            //configuration.MessageHandlers.Add(new HypermediaRouter(baseAddress));
            //configuration.MessageHandlers.Add(new CompressionHandler());
            //configuration.MessageHandlers.Add(new RequireHttpsHandler());
            configuration.MessageHandlers.Add(new AcceptNegotiationHandler(configuration));
        }

        private static void ConfigureDefaultFormatters(HttpConfiguration configuration)
        {
            configuration.Formatters.Clear();
            configuration.Formatters.Add(new JsonFormatter());
            configuration.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());
        }
    }
}