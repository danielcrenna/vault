using NaiveChain.Models;
using NaiveCoin.Models;

namespace NaiveCoin.Tests.Fixtures
{
    public class CoinSettingsFixture
    {
        public CoinSettingsFixture()
        {
            Value = new CoinSettings
            {
                Name = "NaiveCoin",
                FeePerTransaction = 1L,
                GenesisBlock = new CurrencyBlock
				{
                    Index = 0L,
                    PreviousHash = "0",
                    Timestamp = 1465154705L,
                    Nonce = 0L,
					Objects = new BlockObject[] { },
                    Transactions = new[]
                    {
                        new Transaction
                        {
                            Id = "63ec3ac02f822450039df13ddf7c3c0f19bab4acd4dc928c62fcd78d5ebc6dba"
                        }
                    }
                },
				Mining = new CoinSettings.MiningSettings
				{
					MiningReward = 5000000000L
				},
				ProofOfWork = new CoinSettings.ProofOfWorkSettings
				{
					BaseDifficulty = 2147483647L,
					EveryXBlocks = 5,
					PowCurve = 5
	            },
			};
        }

        public CoinSettings Value { get; set; }
    }
}