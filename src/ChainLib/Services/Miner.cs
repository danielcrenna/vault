using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChainLib.Models;

namespace ChainLib.Services
{
    public class Miner
    {
        private readonly IBlockchain<Block> _blockchain;
        private readonly IProofOfWork _proofOfWork;
	    private readonly ILogger<Miner> _logger;

		public Miner(IBlockchain<Block> blockchain, IProofOfWork proofOfWork, ILogger<Miner> logger)
        {
            _blockchain = blockchain;
            _proofOfWork = proofOfWork;
            _logger = logger;
        }

        public async Task<Block> MineAsync(string address)
        {
	        var lastBlock = await _blockchain.GetLastBlockAsync();
	        var baseBlock = GenerateNextBlock(lastBlock);
			var difficulty = _blockchain.GetDifficulty(baseBlock.Index.GetValueOrDefault());

	        return _proofOfWork.ProveWorkFor(baseBlock, difficulty);
        }

        private static Block GenerateNextBlock(Block previousBlock)
        {
            var index = previousBlock.Index + 1;
            var previousHash = previousBlock.Hash;
            var timestamp = DateTimeOffset.UtcNow.Ticks;
			
            return new Block
			{
                Index = index,
                Nonce = 0,
                PreviousHash = previousHash,
                Timestamp = timestamp
            };
        }
    }
}