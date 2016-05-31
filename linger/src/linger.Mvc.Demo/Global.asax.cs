using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using tophat;

namespace linger.Mvc.Demo
{
    public class MvcApplication : HttpApplication
    {
        public static List<string> Bucket { get; set; }

        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InstallSqlServer();
            InstallLinger(DatabaseDialect.SqlServer);
        }

        private static void InstallLinger(DatabaseDialect dialect)
        {
            var repository = new DatabaseJobRepository(dialect, () => UnitOfWork.Current);
            repository.InstallSchema();
            Linger.Backend = repository;
            Linger.StartWorker();
        }

        private static void InstallSqlServer()
        {
            Database.Install<SqlServerConnectionFactory>(ConfigurationManager.ConnectionStrings["Linger"].ConnectionString, ConnectionScope.AlwaysNew);
        }

        private static void InstallSqlite()
        {
            var database = HostingEnvironment.MapPath(@"~\App_Data\demo.s3db");
            if (File.Exists(database))
            {
                File.Delete(database);
            }
            var cs = string.Format("Data Source={0};Version=3;New=True;", database);
            Database.Install<SqliteConnectionFactory>(cs, ConnectionScope.AlwaysNew);
        }
    }
}
