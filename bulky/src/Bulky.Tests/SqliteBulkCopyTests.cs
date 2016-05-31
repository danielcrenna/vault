using NUnit.Framework;
using bulky.Tests.Fixtures;

namespace bulky.Tests
{
    [TestFixture]
    public class SqliteBulkCopyTests : SqliteFixture
    {
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Bulk_copy_n_records_directly(int trials)
        {
            BulkCopyFixture.BulkCopyUsers(trials);
            BulkCopyFixture.BulkCopyUsers(trials, trace: true);
        }
    }
}