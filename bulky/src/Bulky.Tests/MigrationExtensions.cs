using System;
using System.IO;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace bulky.Tests
{
    internal static class Migrator
    {
        static Migrator()
        {
            Builder = CreateMigratorMethod();
        }

        private static Lazy<IMigrationService> Builder { get; set; }
        private static Lazy<IMigrationService> CreateMigratorMethod()
        {
            return new Lazy<IMigrationService>(() => new MigrationService());
        }

        public static void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false)
        {
            Builder.Value.MigrateToLatest(databaseType, connectionString, profile, assembly, trace);
        }

        public static void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false)
        {
            Builder.Value.MigrateToVersion(databaseType, connectionString, version, profile, assembly, trace);
        }
    }

    public static class DatabaseType
    {
        public static readonly string SqlServer = "sqlserver";
        public static readonly string SqlServerCe = "sqlserver";
        public static readonly string Sqlite = "sqlite";
        public static readonly string MySql = "mysql";
        public static readonly string Oracle = "oracle";

        // Add all the others FluentMigrator supports...
    }

    public interface IMigrationService
    {
        void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false);
        void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false);
    }

    public class MigrationService : IMigrationService
    {
        public void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false)
        {
            MigrateToVersion(databaseType, connectionString, 0, profile, assembly);
        }

        public void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false)
        {
            if (databaseType == "sqlite")
            {
                CopyInteropAssemblyByPlatform();
            }
            assembly = assembly ?? Assembly.GetExecutingAssembly();
            var announcer = trace ? new TextWriterAnnouncer(Console.Out) : (IAnnouncer)new NullAnnouncer();
            var context = new RunnerContext(announcer)
            {
                Connection = connectionString,
                Database = databaseType,
                Target = assembly.FullName,
                Version = version,
                Profile = profile
            };
            var executor = new TaskExecutor(context);
            executor.Execute();
        }

        private const string SQLiteAssembly = "SQLite.Interop.dll";
        public static void CopyInteropAssemblyByPlatform()
        {
            var baseDir = WhereAmI();
            var destination = Path.Combine(baseDir, SQLiteAssembly);
            if (File.Exists(destination))
            {
                return;
            }
            var arch = Environment.Is64BitProcess ? "x64" : "x86";
            var path = Path.Combine(arch, SQLiteAssembly);
            var source = Path.Combine(baseDir, path);
            File.Copy(source, destination, true);
        }
        internal static string WhereAmI()
        {
            var dir = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var fi = new FileInfo(dir.AbsolutePath);
            return fi.Directory != null ? fi.Directory.FullName : null;
        }
    }

    internal static class MigrationExtensions
    {
        public static ICreateTableWithColumnSyntax Timestamps(this ICreateTableWithColumnSyntax migration)
        {
            return migration
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable();
        }

        public static ICreateTableWithColumnSyntax EffectiveDates(this ICreateTableWithColumnSyntax migration)
        {
            return migration
                .WithColumn("StartDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("EndDate").AsDateTime().Nullable();
        }
    }
}