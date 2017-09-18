using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NaiveCoin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        // https://github.com/aspnet/KestrelHttpServer/issues/1998
        // https://github.com/aspnet/MetaPackages/issues/221

        public static IWebHost BuildWebHost(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .ConfigureAppConfiguration(ConfigureAppSettings)
                .ConfigureLogging(ConfigureLogging)
                .UseStartup<Startup>()
                .Build();
        }

        private static void ConfigureLogging(WebHostBuilderContext hostingContext, ILoggingBuilder logging)
        {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
        }

        private static void ConfigureAppSettings(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            
            config.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            config.AddEnvironmentVariables();
        }
    }
}