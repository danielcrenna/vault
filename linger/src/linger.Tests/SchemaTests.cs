using NUnit.Framework;
using tophat;

namespace linger.Tests
{
    [TestFixture]
    public class SchemaTests : SqliteFixture
    {
        [Test]
        public void Can_safely_call_create()
        {
            Schema.Install(DatabaseDialect.Sqlite, UnitOfWork.Current);
            Schema.Install(DatabaseDialect.Sqlite, UnitOfWork.Current);
            Schema.Install(DatabaseDialect.Sqlite, UnitOfWork.Current);
        }
    }
}