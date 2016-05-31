using System;
using NUnit.Framework;

namespace linger.Tests
{
    [TestFixture, Ignore("One offs for SQL generation only")]
    public class MigrationTests
    {
        [Test]
        public void Emit_mssql()
        {
            var cs = "Data Source=(local);Initial Catalog=Linger;Integrated Security=true";
            var svc = new MigrationHelper("sqlserver");
            svc.Migrate(cs);
        }

        [Test]
        public void Emit_sqlite()
        {
            var database = string.Format("{0}.s3db", Guid.NewGuid());
            var cs = string.Format("Data Source={0};Version=3;New=True;", database);
            var svc = new MigrationHelper("sqlite");
            svc.Migrate(cs);
        }

        [Test]
        public void Emit_mysql()
        {
            var cs = string.Format("Database=Linger;User=root;Password=password");
            var svc = new MigrationHelper("mysql");
            svc.Migrate(cs);
        }
    }
}
