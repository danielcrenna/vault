using System;
using System.IO;
using NUnit.Framework;

namespace tophat.Tests
{
    public class DatabaseFixture
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            var files = Directory.GetFiles("/", "*.sqdb", SearchOption.TopDirectoryOnly);
            foreach(var db in files)
            {
                File.Delete(db);
            }
        }

        public static string CreateConnectionString()
        {
            var database = string.Format("{0}.sqdb", Guid.NewGuid());
            var connectionString = string.Format("Data Source={0};Version=3;New=True;", database);
            return connectionString;
        }
    }
}