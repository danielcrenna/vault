using System;
using System.Configuration;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using cohort.API;

namespace cohort.Api.SelfHost
{
    public class ApiHost
    {
        internal HttpConfiguration Configuration { get; private set; }
        
        //private string _username;
        public string BaseAddress { get; private set; }
        public string Machine { get; private set; }
        public string Port { get; private set; }

        private HttpSelfHostServer _server;
        
        public WindowsPrincipal GetPrincipalIdentity()
        {
            var identity = WindowsIdentity.GetCurrent();
            if (identity == null)
            {
                Console.WriteLine("Unable to resolve Windows identity.");
                return null;
            }
            var principal = new WindowsPrincipal(identity);
            return principal;
        }

        public bool Initialize()
        {
            var principal = GetPrincipalIdentity();
            if(principal == null)
            {
                return false;
            }
            /* -> Belongs in a separate execution context (using scope for ACL executions)
            var admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!admin)
            {
                Console.WriteLine("API startup requires administrator privileges.");
                return false;
            }
            _username = principal.Identity.Name;
            */


            Port = ConfigurationManager.AppSettings["ServicePort"] ?? "8181";
            Machine = Environment.MachineName;
            BaseAddress = String.Concat("http://", Machine, ":", Port, "/");
            Thread.CurrentPrincipal = principal;
            
            var host = ConfigureSelfHost();
            Configuration = host;

            _server = new HttpSelfHostServer(host);
            CohortApi.Register(Configuration);

            return true;
        }
        
        public void Start()
        {
            if (!Initialize()) return;
            _server.OpenAsync().Wait();
        }

        public HttpClient CreateTestClient()
        {
            var server = new HttpServer(Configuration);
            var client = new HttpClient(server);
            return client;
        }

        private HttpSelfHostConfiguration ConfigureSelfHost()
        {
            var config = new HttpSelfHostConfiguration(BaseAddress);
            //Utils.Execute("netsh.exe", String.Format("http add urlacl url=http://+:{0}/ user={1}", Port, _username));
            return config;
        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.CloseAsync().Wait();
            }
            //Utils.Execute("netsh.exe", String.Format("http delete urlacl url=http://+:{0}/", Port));
        }
    }
}