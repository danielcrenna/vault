using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace hammock2.Tests.API
{
    public static class FakeServer
    {
        private static string _baseAddress;
        private static HttpConfiguration _config;
        private static Action<HttpSelfHostConfiguration> _configure;
        
        public static HttpClient CreateClientForAServerOnPort(int port)
        {
            _baseAddress = string.Format("http://localhost:{0}/", port);
            _config = _config ?? ConfigureHost();
            var server = new HttpServer(_config);
            var client = new HttpClient(server);
            return client;
        }

        private static HttpSelfHostConfiguration ConfigureHost()
        {
            var config = new HttpSelfHostConfiguration(_baseAddress);
            if (_configure != null)
            {
                _configure(config);
            }
            return config;
        }

        public static void Configure(Action<HttpSelfHostConfiguration> config)
        {
            _configure = config;
        }
    }
}