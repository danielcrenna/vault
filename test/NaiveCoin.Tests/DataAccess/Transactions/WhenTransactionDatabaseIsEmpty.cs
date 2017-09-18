using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;
using Xunit;

namespace NaiveCoin.Tests.DataAccess.Transactions
{
    public class WhenTransactionDatabaseIsEmpty : IClassFixture<EmptyTransactionDatabaseFixture>
    {
        public WhenTransactionDatabaseIsEmpty(EmptyTransactionDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public void There_are_no_errors() { }

        public EmptyTransactionDatabaseFixture Fixture { get; set; }
    }
}
