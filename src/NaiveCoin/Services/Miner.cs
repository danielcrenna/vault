using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveChain.Models;
using NaiveCoin.Models;
using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Services
{
    public class Miner
    {
        private readonly ICurrencyBlockchain _blockchain;
        private readonly IProofOfWork _proofOfWork;
	    private readonly CoinSettings _coinSettings;
	    private readonly ILogger<Miner> _logger;

		public Miner(ICurrencyBlockchain blockchain, IProofOfWork proofOfWork, IOptions<CoinSettings> coinSettings, ILogger<Miner> logger)
        {
            _blockchain = blockchain;
            _proofOfWork = proofOfWork;
	        _logger = logger;
            _coinSettings = coinSettings.Value;
        }

        public async Task<CurrencyBlock> MineAsync(string address)
        {
	        var lastBlock = await _blockchain.GetLastBlockAsync();
	        var transactions = _blockchain.StreamAllTransactions();

	        CurrencyBlock baseBlock = GenerateNextBlock(address, lastBlock, transactions);

            return (CurrencyBlock) _proofOfWork.ProveWorkFor(baseBlock, _blockchain.GetDifficulty(baseBlock.Index.GetValueOrDefault()));
        }

        private CurrencyBlock GenerateNextBlock(string address, Block previousBlock, IEnumerable<Transaction> pendingTransactions)
        {
			var index = previousBlock.Index + 1;
            var previousHash = previousBlock.Hash;
            var timestamp = DateTimeOffset.UtcNow.Ticks;

	        _logger?.LogInformation($"Generating next block at index {index}, previousHash={previousHash}");

			// Get the first two available transactions, if there aren't 2, it's empty
			var transactions = pendingTransactions.Take(2).ToList();

            // Add fee transaction
            // INFO: usually it's a fee over transaction size (not amount)
            if (transactions.Count > 0)
            {
				var fee = _coinSettings.FeePerTransaction * transactions.Count;

				_logger?.LogInformation($"Found {transactions.Count} pending transactions, adding them to the new block with fee transaction of {fee}");

	            var id = CryptoUtil.RandomString();

				var feeTransaction = new Transaction
                {
                    Id = id,
                    Hash = null,
                    Type = TransactionType.Fee,
                    Data = new TransactionData
                    {
                        Outputs = new List<TransactionItem>
                        {
                            new TransactionItem
                            {
								Index = 1,
								TransactionId = id,
	                            Type = TransactionDataType.Output,
								Amount = fee, // satoshis format
                                Address = address.FromHex() // INFO: Usually here is a locking script (to check who and when this transaction output can be used), in this case it's a simple destination address     
                            }
                        }
                    }
                };

                transactions.Add(feeTransaction);
            }

            // Add reward transaction of 50 coins
            if (address != null)
            {
				var reward = _coinSettings.Mining.MiningReward;
				_logger?.LogInformation($"Adding reward transaction of {reward}");

	            var id = CryptoUtil.RandomString();

	            var rewardTransaction = new Transaction
                {
                    Id = id,
                    Hash = null,
                    Type = TransactionType.Reward,
                    Data = new TransactionData
                    {
                        Outputs = new List<TransactionItem>
                        {
                            new TransactionItem
                            {
								Index = 1,
								TransactionId = id,
								Type = TransactionDataType.Output,
                                Amount = reward,
                                Address = address.FromHex() // INFO: Usually here is a locking script (to check who and when this transaction output can be used), in this case it's a simple destination address     
                            }
                        }
                    }
                };

                transactions.Add(rewardTransaction);
            }

	        var nextBlock = new CurrencyBlock
	        {
		        Index = index,
		        Nonce = 0,
		        PreviousHash = previousHash,
		        Timestamp = timestamp,
		        Transactions = transactions
	        };

			return nextBlock;
		}
    }
}