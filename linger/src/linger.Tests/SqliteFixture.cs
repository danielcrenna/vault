using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using linger.Tests.Sqlite;
using tophat;

namespace linger.Tests
{
    public abstract class SqliteFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            foreach (var database in Directory.GetFiles(Utils.WhereAmI(), "*.s3db"))
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
            UnitOfWork.Purge();
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
            var database = string.Format("{0}.s3db", Guid.NewGuid());
            var connectionString = string.Format("Data Source={0};Version=3;New=True;", database);
            Database.Install<SqliteConnectionFactory>(connectionString, ConnectionScope.ByThread);

            Schema.Install(DatabaseDialect.Sqlite, UnitOfWork.Current);
            var repository = new DatabaseJobRepository(DatabaseDialect.Sqlite, () => UnitOfWork.Current);
            Linger.Backend = repository;
            repository.InstallSchema();
        }
    }
}
