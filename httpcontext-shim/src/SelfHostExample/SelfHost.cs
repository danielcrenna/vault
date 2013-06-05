using System;
using System.Configuration;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SelfHostExample
{
    /// <summary>
    /// Code taken from http://github.com/danielcrenna/apitize
    /// </summary>
    public class SelfHost
    {
        internal HttpConfiguration Configuration { get; private set; }
        
        private string _username;
        public string BaseAddress { get; private set; }
        public string Machine { get; private set; }
        public string Port { get; private set; }

        private HttpSelfHostServer _server;
        private Action<HttpSelfHostConfiguration> _configure;

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
            if (principal == null)
            {
                return false;
            }
            var admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!admin)
            {
                Console.WriteLine("API startup requires administrator privileges.");
                return false;
            }
            _username = principal.Identity.Name;
            Port = ConfigurationManager.AppSettings["ServicePort"] ?? "8181";
            Machine = Environment.MachineName;
            BaseAddress = String.Concat("http://", Machine, ":", Port, "/");
            Thread.CurrentPrincipal = principal;
            var host = ConfigureSelfHost();
            Configuration = host;
            _server = new HttpSelfHostServer(host);
            return true;
        }

        public void Start()
        {
            if (!Initialize()) return;
            _server.OpenAsync().Wait();
        }
        private HttpSelfHostConfiguration ConfigureSelfHost()
        {
            var config = new HttpSelfHostConfiguration(BaseAddress);
            Execute("netsh.exe", String.Format("http add urlacl url=http://+:{0}/ user={1}", Port, _username));
            if (_configure != null)
            {
                _configure(config);
            }
            return config;
        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.CloseAsync().Wait();
            }
            Execute("netsh.exe", String.Format("http delete urlacl url=http://+:{0}/", Port));
        }

        /// <summary>
        /// Perform actions against a <see cref="HttpSelfHostConfiguration"/>, generally useful for setting host-specific configuration options
        /// </summary>
        /// <param name="config"></param>
        public void Configure(Action<HttpSelfHostConfiguration> config)
        {
            _configure = config;
        }

        internal static void Execute(string filename, string arguments, bool trace = false)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    Verb = "runas"
                }
            };
            p.Start();
            p.WaitForExit(30000);
            if (!trace) return;
            var output = p.StandardOutput.ReadToEnd();
            Console.Write(output);
        }
    }
}