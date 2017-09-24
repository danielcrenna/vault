using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Models;
using NaiveCoin.Services;
using NaiveCoin.Tests.Fixtures.DataAccess.Blocks;

namespace NaiveCoin.Tests.Fixtures
{
    public class BlockchainFixture
    {
        public BlockchainFixture()
        {
            var coinSettings =new OptionsWrapper<CoinSettings>(new CoinSettingsFixture().Value);
            var blocks = new BlockDatabaseWithGenesisBlockFixture();
            var transactions = new EmptyTransactionDatabaseFixture();
            var hashProvider = new ObjectHashProviderFixture();
            var pow = new SimpleProofOfWork(hashProvider.Value, coinSettings);

            var factory = new LoggerFactory();
            factory.AddConsole();
            
            Value = new Blockchain(
                coinSettings,
                blocks.Value,
                pow,
                transactions.Value,
                hashProvider.Value,
                null, null);
        }

        public Blockchain Value { get; set; }
    }
}