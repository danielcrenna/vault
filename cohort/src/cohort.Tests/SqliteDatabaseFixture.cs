using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using tophat;
using tuxedo;
using tuxedo.Dialects;

namespace cohort.Tests
{
    public class SqliteDatabaseFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            foreach(var database in Directory.GetFiles(WhereAmI(), "*.sq3db"))
            {
                DeleteDatabase(database);
            }
        }

        [SetUp]
        public void SetUp()
        {
            CreateTestDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            UnitOfWork.Current.Close();
            UnitOfWork.Current.Dispose();
        }

        private static void DeleteDatabase(string databaseName)
        {
            if (!File.Exists(databaseName))
            {
                return;
            }
            var i = 10;
            while (IsDatabaseInUse(databaseName) && i > 0)
            {
                i--;
                Thread.Sleep(1000);
            }
            if (i > 0)
            {
                File.Delete(databaseName);
            }
        }

        private static bool IsDatabaseInUse(string databaseName)
        {
            FileStream fs = null;
            try
            {
                var fi = new FileInfo(databaseName);
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        private static void CreateTestDatabase()
        {
            var database = string.Format("{0}.sq3db", Guid.NewGuid());
            var connectionString = string.Format("Data Source={0};Version=3;New=True;", database);
            Database.Install<SqliteConnectionFactory>(connectionString, ConnectionScope.ByThread);

            // Find a way to hide this in the library
            Tuxedo.Dialect = new SqliteDialect();
        }

        internal static string WhereAmI()
        {
            var dir = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var fi = new FileInfo(dir.AbsolutePath);
            return fi.Directory != null ? fi.Directory.FullName : null;
        }
    }
}