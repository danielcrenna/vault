using System;
using System.Configuration;
using System.Linq;
using Dapper;
using NUnit.Framework;
using bulky.Tests.MySql;
using tophat;
using tuxedo.Dialects;

namespace bulky.Tests.Fixtures
{
    public class MySqlFixture
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Bulky.BulkCopier = new MySqlBulkCopy();
            BulkCopyFixture.Dialect = new MySqlDialect();
        }

        [SetUp]
        public void SetUp()
        {
            UnitOfWork.Purge();
            ConfigureTestDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            UnitOfWork.Current.Execute(string.Format("DROP DATABASE `{0}`", UnitOfWork.Current.Database));
            UnitOfWork.Purge();
        }

        private static void ConfigureTestDatabase()
        {
            var database = CreateDatabase();
            var connectionString = string.Format("Server=localhost;Uid={0};Pwd={1};Database={2};", ConfigurationManager.AppSettings["MySQLUser"], ConfigurationManager.AppSettings["MySQLPassword"], database);
            Database.Install<MySqlConnectionFactory>(connectionString, ConnectionScope.ByThread);
            new MigrationService().MigrateToLatest("mysql", connectionString);
            UnitOfWork.Current.Query("SELECT COUNT(*) AS `C` FROM `User`").Single();
            Assert.AreEqual(UnitOfWork.Current.Database, database.ToString());
        }
        
        private static Guid CreateDatabase()
        {
            var database = Guid.NewGuid();
            var connectionString = string.Format("Server=localhost;Uid={0};Pwd={1};", ConfigurationManager.AppSettings["MySQLUser"], ConfigurationManager.AppSettings["MySQLPassword"]);
            var factory = new MySqlConnectionFactory { ConnectionString = connectionString };
            using (var connection = factory.CreateConnection())
            {
                connection.Open();
                var sql = string.Format("CREATE DATABASE `{0}`", database);
                connection.Execute(sql);
                sql = string.Format("USE `{0}`", database);
                connection.Execute(sql);
            }
            return database;
        }
    }
}
