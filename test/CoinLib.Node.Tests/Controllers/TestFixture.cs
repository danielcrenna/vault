using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoinLib.Tests.Controllers
{
    public class TestFixture<T> : IDisposable
    {
        private readonly TestServer _server;

        public HttpClient Client { get; }

        public TestFixture()
        {
            var builder = new WebHostBuilder()
                .UseConfiguration(UseConfiguration())
                .ConfigureServices(ConfigureServices)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .UseEnvironment("InteractionTest")
                .UseStartup(typeof(T));

            _server = new TestServer(builder);
            Client = _server.CreateClient();
        }

        private IConfiguration UseConfiguration()
        {
            return BuildConfiguration();
        }

        protected virtual IConfiguration BuildConfiguration()
        {
            var config = new ConfigurationBuilder();
            config.AddJsonFile($"appsettings.InteractionTest.json", true, true);
            config.AddEnvironmentVariables();
            return config.Build();
        }

        protected virtual void ConfigureServices(IServiceCollection services) { }

        protected virtual void ConfigureLogging(WebHostBuilderContext hostingContext, ILoggingBuilder logging)
        {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddConsole();
        }

        protected virtual void ConfigureAppConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config) { }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}