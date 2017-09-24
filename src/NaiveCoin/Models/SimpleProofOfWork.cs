using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NaiveCoin.Models
{
    public class SimpleProofOfWork : IProofOfWork
    {
        private readonly ILogger<SimpleProofOfWork> _logger;
        private readonly CoinSettings _coinSettings;

        public SimpleProofOfWork(IOptions<CoinSettings> coinSettings, ILogger<SimpleProofOfWork> logger = null)
        {
            _logger = logger;
            _coinSettings = coinSettings.Value;
        }

        public double GetDifficulty(long index)
        {
            var pow = _coinSettings.ProofOfWork;

            // INFO: The difficulty is the formula that naivecoin choose to check the proof a work, this number is later converted to base 16 to represent the minimal initial hash expected value

            // Calculates the difficulty based on the index since the difficulty value increases every two blocks        
            return Math.Max(
                Math.Floor(pow.BaseDifficulty / Math.Pow(Math.Floor((index + 1D) / pow.EveryXBlocks) + 1,
                               pow.PowCurve)), 0);
        }

        public Block ProveWorkFor(Block block, double difficulty)
        {
            _logger?.LogInformation($"Mining a new block with difficulty '{difficulty}'");

            double? blockDifficulty;
            var sw = Stopwatch.StartNew();

            // INFO: Every cryptocurrency has a different way to prove work, this is a simple hash sequence

            // Loop incrementing the nonce to find the hash at desired difficulty
            do
            {
                block.Timestamp = DateTimeOffset.UtcNow.Ticks;
                block.Nonce++;
                blockDifficulty = block.GetDifficulty();
            } while (blockDifficulty >= difficulty);

            _logger?.LogInformation($"Block found: time '{sw.Elapsed.TotalSeconds} sec' difficulty '{difficulty}' hash '{block.Hash}' nonce '{block.Nonce}'");
            return block;
        }

    }
}