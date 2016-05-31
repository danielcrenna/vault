using NUnit.Framework;

namespace tophat.Tests
{
    [TestFixture]
    public class DataContextTests : DatabaseFixture
    {
        [Test]
        public void Using_data_context_is_an_option()
        {
            var cs = CreateConnectionString();
            using (var db = new SqliteDataContext(cs))
            {
                Assert.IsNotNull(db);
            }
        }
    }
}