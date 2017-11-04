using CoinLib.Models;

namespace CoinLib
{
    public class CoinSettings
    {
        public string Name { get; set; }
        public long FeePerTransaction { get; set; }
        public CurrencyBlock GenesisBlock { get; set; }
        public ProofOfWorkSettings ProofOfWork { get; set; }
        public MiningSettings Mining { get; set; }

        public class ProofOfWorkSettings
        {
            public long BaseDifficulty { get; set; }
            public int EveryXBlocks { get; set; }
            public int PowCurve { get; set; }
        }

        public class MiningSettings
        {
            public long MiningReward { get; set; }
        }
    }
}