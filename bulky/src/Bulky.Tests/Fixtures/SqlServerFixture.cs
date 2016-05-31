using System;
using Dapper;
using NUnit.Framework;
using tophat;
using tuxedo.Dialects;

namespace bulky.Tests.Fixtures
{
    public class SqlServerFixture
    {
        private string _database;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Bulky.BulkCopier = new SqlServerBulkCopy();
            BulkCopyFixture.Dialect = new SqlServerDialect();
        }

        [SetUp]
        public void SetUp()
        {
            UnitOfWork.Purge();
            _database = ConfigureTestDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            UnitOfWork.Current.Execute("USE master");
            UnitOfWork.Current.Execute(string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", _database));
            UnitOfWork.Current.Execute(string.Format("DROP DATABASE [{0}]", _database));
            UnitOfWork.Purge();
        }

        private static string ConfigureTestDatabase()
        {
            var database = CreateDatabase();
            var connectionString = string.Format("Data Source=localhost;Initial Catalog={0};Integrated Security=true", database);
            Database.Install<SqlServerConnectionFactory>(connectionString, ConnectionScope.ByThread);
            new MigrationService().MigrateToLatest("sqlserver", connectionString);
            return database.ToString();
        }

        private static Guid CreateDatabase()
        {
            var database = Guid.NewGuid();
            var connectionString = string.Format("Data Source=localhost;Integrated Security=true;");
            var factory = new SqlServerConnectionFactory { ConnectionString = connectionString };
            using (var connection = factory.CreateConnection())
            {
                connection.Open();
                var sql = string.Format("CREATE DATABASE [{0}]", database);
                connection.Execute(sql);
            }
            return database;
        }
    }
}