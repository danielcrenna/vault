using System;
using System.IO;
using System.Reflection;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace linger.Tests
{
    public class MigrationHelper
    {
        private readonly string _databaseType;

        public MigrationHelper(string databaseType)
        {
            _databaseType = databaseType;
        }

        public virtual void Migrate(string connectionString, Assembly assembly = null)
        {
            if (_databaseType == "sqlite")
            {
                CopyInteropAssemblyByPlatform();
            }
            assembly = assembly ?? Assembly.GetExecutingAssembly();
            var announcer = new TextWriterAnnouncer(Console.Out) {ShowSql = true, ShowElapsedTime = false};
            var context = new RunnerContext(announcer)
            {
                Connection = connectionString,
                Database = _databaseType,
                Target = assembly.FullName,
                Profile = "linger",
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
}