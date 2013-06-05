using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HttpContextShim;

namespace SelfHostExample
{
    class Program
    {
        static void Main()
        {
            var host = new SelfHost();
            host.Configure(configuration =>
            {
                configuration.MessageHandlers.Add(new HttpContextHandler());
                configuration.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
            
            try
            {
                host.Start();
                Console.WriteLine("API ready on http://{0}:{1}.", host.Machine, host.Port);
                Console.WriteLine("Press enter to terminate.");

                // Exercises the API through the HttpContext
                Task.Factory.StartNew(() => Ping(host));

                Console.ReadLine();
            }
            finally
            {
                host.Stop();
            }
        }

        private static void Ping(SelfHost host)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(string.Concat("http://", host.Machine, ":", host.Port));

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(client.BaseAddress, "api/ping");

            var response = client.SendAsync(request).Result;
            Console.WriteLine(response.RequestMessage);
        }
    }
}
