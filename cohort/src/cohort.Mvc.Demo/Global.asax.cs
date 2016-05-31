using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using cohort.API;
using cohort.Logging;
using cohort.Migrations;
using cohort.Sqlite;
using linger.DataAccess;
using linger;
using tophat;

namespace cohort.Mvc.Demo
{
    public class WebApiApplication : HttpApplication
    {
        private static bool _configured;

        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            if (_configured) return;
            
            var connectionString = ConfigureDatabase();
            Cohort.Initialize(DatabaseType.Sqlite, connectionString, cs => UnitOfWork.Current);
            CohortApi.Register(GlobalConfiguration.Configuration);
            CohortMvc.Register();
            
            Linger.Backend = () => new DatabaseJobRepository(DatabaseDialect.Sqlite, () => UnitOfWork.Current);
            Schema.Install(UnitOfWork.Current, DatabaseDialect.Sqlite);
            Func<bool> hello = () =>
            {
                Logger.Info("Hello, world!");
                return true;
            };
            hello.PerformAsync();
            Linger.StartWorker();

            _configured = true;
        }

        private static string ConfigureDatabase()
        {
            var database = HostingEnvironment.MapPath(@"~\App_Data\cohort.s3db");
#if DEBUG
            if (File.Exists(database))
            {
                File.Delete(database);
            }
#endif
            var connectionString = string.Format("Data Source={0};Version=3;New=True;", database);
            Database.Install<SqliteConnectionFactory>(connectionString);
            return connectionString;
        }
    }
}