using Microsoft.Extensions.Logging;
using NaiveCoin.Models;
using NaiveCoin.Services;
using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;

namespace NaiveCoin.Tests.Fixtures
{
    public class BlockchainFixture
    {
        public BlockchainFixture()
        {
            var settings = new CoinSettingsFixture();
            var blocks = new BlockDatabaseWithGenesisBlockFixture();
            var transactions = new EmptyTransactionDatabaseFixture();
            var hashProvider = new ObjectHashProviderFixture();

            var factory = new LoggerFactory();
            factory.AddConsole();
            
            Value = new Blockchain(settings.Value,
                blocks.Value,
                transactions.Value,
                hashProvider.Value,
                null, null);
        }

        public Blockchain Value { get; set; }
    }
}