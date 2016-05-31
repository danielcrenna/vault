using System;
using Dapper;
using NUnit.Framework;
using ab.Tests.Schema;
using tophat;

namespace ab.Tests
{
    public class SqlServerFixture
    {
        private string _database;

        [SetUp]
        public void SetUp()
        {
            UnitOfWork.Purge();
            _database = ConfigureTestDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            var db = UnitOfWork.Current;
            db.Execute("USE master");
            db.Execute(string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", _database));
            db.Execute(string.Format("DROP DATABASE [{0}]", _database));
            UnitOfWork.Purge();
        }

        private static string ConfigureTestDatabase()
        {
            var database = CreateDatabase();
            var connectionString = string.Format("Data Source=localhost;Initial Catalog={0};Integrated Security=true", database);
            Database.Install<SqlServerConnectionFactory>(connectionString, ConnectionScope.ByThread);
            Migrations.MigrateToLatest(DatabaseType.SqlServer, connectionString, "AB");
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