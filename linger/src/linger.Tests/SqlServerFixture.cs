using System;
using Dapper;
using NUnit.Framework;
using tophat;

namespace linger.Tests
{
    public abstract class SqlServerFixture
    {
        protected string Database;
        protected string ConnectionString;

        [SetUp]
        public void SetUp()
        {
            UnitOfWork.Purge();
            ConnectionString = ConfigureTestDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            DropDatabaseUnderTest();
        }

        private void DropDatabaseUnderTest()
        {
            UnitOfWork.Current.Execute("USE master");
            UnitOfWork.Current.Execute(string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", Database));
            UnitOfWork.Current.Execute(string.Format("DROP DATABASE [{0}]", Database));
            UnitOfWork.Purge();
        }

        private string ConfigureTestDatabase()
        {
            var database = CreateDatabase();
            var connectionString = string.Format("Data Source=localhost;Initial Catalog={0};Integrated Security=true", database);
            tophat.Database.Install<SqlServerConnectionFactory>(connectionString, ConnectionScope.ByThread);
            Database = database.ToString();

            var factory = new SqlServerConnectionFactory { ConnectionString = connectionString };
            using (var connection = factory.CreateConnection())
            {
                connection.Open();
                Schema.Install(DatabaseDialect.SqlServer, connection);
            }

            return connectionString;
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