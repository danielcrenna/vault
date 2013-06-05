using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace hammock2.Tests.API
{
    public class FakeController : ApiController
    {
        public static void RegisterRoutes(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(name: "Request_parameterless_get", routeTemplate: "", defaults: new { Controller = "Fake", Action = "GetWithNoParameters" });
            config.Routes.MapHttpRoute(name: "Can_get_an_entity_from_a_url_on_first_node", routeTemplate: "one", defaults: new { Controller = "Fake", Action = "GetEntity", filename = "twitter_users_show.json" });
            config.Routes.MapHttpRoute(name: "Can_get_an_entity_from_a_url_on_second_node", routeTemplate: "one/two", defaults: new { Controller = "Fake", Action = "GetEntity", filename = "twitter_users_show.json" });
            config.Routes.MapHttpRoute(name: "Can_get_an_entity_from_a_url_on_dot_node", routeTemplate: "one/two/three.four", defaults: new { Controller = "Fake", Action = "GetEntity", filename = "twitter_users_show.json" });
        }

        public HttpResponseMessage GetWithNoParameters()
        {
            return new HttpResponseMessage();
        }

        public HttpResponseMessage GetEntity(string filename)
        {
            var content = new StringContent(File.ReadAllText(Path.Combine("API/Entities", filename)), Encoding.UTF8);
            var response = new HttpResponseMessage();
            response.Content = content;
            return response;
        }
    }
}