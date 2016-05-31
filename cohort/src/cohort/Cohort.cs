using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using bulky;
using cohort.Configuration;
using cohort.Email;
using cohort.Logging;
using cohort.Migrations;
using cohort.Models;
using cohort.Services;
using container;
using tuxedo;
using tuxedo.Dialects;

namespace cohort
{
    public static partial class Cohort
    {
        private static readonly object Sync = new object();
        internal static CohortRoleProvider Roles { get; private set; }
        
        public static Container Container
        {
            get { return Config.Container; }
        }

        private static bool _initialized;

        private static string _connectionString;
        public static IDbConnection Database
        {
            get
            {
                var connection = Container.Resolve<Func<string, IDbConnection>>();
                return connection(_connectionString);
            }
        }

        public static void Initialize(string databaseType, string connectionString, Func<string, IDbConnection> connection)
        {
            if (_initialized)
            {
                return;
            }
            lock(Sync)
            {
                if (_initialized)
                {
                    return;
                }

                var wrapped = new Func<string, IDbConnection>(cs => new ProfiledDbConnection((DbConnection) connection(cs), MiniProfiler.Current));

                var types = RegisterCoreServices(wrapped);

                Roles = new CohortRoleProvider();

                InstallDatabase(databaseType, connectionString);
                
                Site = Container.Resolve<SiteContext>();

                RegisterPluginDependentServices(types);
                
                // This is gross, just do the work once
                var r = Container.Resolve<RoleService>();
                r.SuperUserRole = Site.Auth.SuperUserRole;
                r.SuperUserPassword = Site.Auth.SuperUserPassword;
                r.SuperUserEmail = Site.Auth.SuperUserEmail;
                r.AdminRole = Site.Auth.AdminRole;

                ConfigureLogging();
            }
            _initialized = true;
        }

        public static SiteContext Site { get; private set; }

        private static List<Type> RegisterCoreServices(Func<string, IDbConnection> connection)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(assembly => assembly.GetTypes()).ToList();

            Container.Register(r => connection);

            LoadFromAvailableTypes<IAuthenticationService>(types);
            LoadFromAvailableTypes<ISecurityService>(types);

            LoadFromAvailableTypes<IRoleRepository>(types);
            LoadFromAvailableTypes<IUserRepository>(types);
            LoadFromAvailableTypes<IProfileRepository>(types);
            LoadFromAvailableTypes<IConfigRepository>(types);
            LoadFromAvailableTypes<ILogRepository>(types);
            LoadFromAvailableTypes<IEmailRepository>(types);
            LoadFromAvailableTypes<ITokenRepository>(types);

            return types;
        }

        private static void RegisterPluginDependentServices(List<Type> types)
        {
            RegisterEmail(types);
        }

        private static void RegisterEmail(List<Type> types)
        {
            LoadFromAvailableTypes<IEmailTemplateEngine>(types);
            var provider = Site.Email.Provider;
            var found = types.FirstOrDefault(t => typeof (IEmailProvider).IsAssignableFrom(t) && t.Name.ToLower().StartsWith(provider));
            if (found != null)
            {
                if (!string.IsNullOrWhiteSpace(Site.Email.ProviderKey))
                {
                    Container.Register(r => Activator.CreateInstance(found, new object[] {Site.Email.ProviderKey}) as IEmailProvider);
                }
                else
                {
                    Container.Register(r => Activator.CreateInstance(found) as IEmailProvider);
                }
            }
            LoadFromAvailableTypes<IEmailService>(types);
        }

        private static void LoadFromAvailableTypes<T>(IEnumerable<Type> types) where T : class
        {
            var type = typeof(T);
            var ur = types.Where(t => type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();
            if(!ur.Any())
            {
                throw new InvalidOperationException("Current AppDomain does not have an available implementation of " + type.Name);
            }
            var concreteType = ur.First();
            if(!Container.CanResolve(concreteType))
            {
                Container.Register(r => Activator.CreateInstance(concreteType) as T);
            }
        }

        private static void ConfigureLogging()
        {
            // This is supposed to register a custom target dynamically but does not work...
            // ConfigurationItemFactory.Default.Targets.RegisterDefinition("CohortLog", typeof (CohortLogTarget));
            
            Logger.Info("============================");
            Logger.Info("    Starting application    ");
            Logger.Info("============================");
        }

        private static void InstallDatabase(string databaseType, string connectionString)
        {
            Migrator.MigrateToLatest(databaseType, connectionString, assembly: typeof (Cohort).Assembly);
            Tuxedo.Dialect = new SqliteDialect();
            Bulky.BulkCopier = new SqliteBulkCopy();
        }
        
        internal static string WhereAmI()
        {
            var dir = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var fi = new FileInfo(dir.AbsolutePath);
            return fi.Directory != null ? fi.Directory.FullName : null;
        }
    }
}
