using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveChain;
using NaiveCoin.Models;
using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Services
{
    public class Miner
    {
        private readonly Blockchain _blockchain;
        private readonly IProofOfWork _proofOfWork;
        private readonly ILogger<Miner> _logger;
        private readonly CoinSettings _coinSettings;

        public Miner(Blockchain blockchain, IProofOfWork proofOfWork, IOptions<CoinSettings> coinSettings, ILogger<Miner> logger)
        {
            _blockchain = blockchain;
            _proofOfWork = proofOfWork;
            _logger = logger;
            _coinSettings = coinSettings.Value;
        }

        public async Task<CurrencyBlock> MineAsync(string address)
        {
            var baseBlock = GenerateNextBlock(address, await _blockchain.GetLastBlockAsync(), _blockchain.StreamAllTransactions());

            return _proofOfWork.ProveWorkFor(baseBlock, _blockchain.GetDifficulty(baseBlock.Index.GetValueOrDefault()));
        }

        private CurrencyBlock GenerateNextBlock(string address, CurrencyBlock previousBlock, IEnumerable<Transaction> pendingTransactions)
        {
            var index = previousBlock.Index + 1;
            var previousHash = previousBlock.Hash;
            var timestamp = DateTimeOffset.UtcNow.Ticks;

            // Get the first two available transactions, if there aren't 2, it's empty
            var transactions = pendingTransactions.Take(2).ToList();

            // Add fee transaction (1 satoshi per transaction)
            // INFO: usually it's a fee over transaction size (not amount)
            if (transactions.Count > 0)
            {
                var feeTransaction = new Transaction
                {
                    Id = CryptoUtil.RandomString(64),
                    Hash = null,
                    Type = TransactionType.Fee,
                    Data = new TransactionData
                    {
                        Inputs = new TransactionItem[] {},
                        Outputs = new[]
                        {
                            new TransactionItem
                            {
                                Amount = _coinSettings.FeePerTransaction * transactions.Count, // satoshis format
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
                var rewardTransaction = new Transaction
                {
                    Id = CryptoUtil.RandomString(64),
                    Hash = null,
                    Type = TransactionType.Fee,
                    Data =
                    {
                        Inputs = new TransactionItem[] {},
                        Outputs = new[]
                        {
                            new TransactionItem
                            {
                                Amount = _coinSettings.Mining.MiningReward,
                                Address = address.FromHex() // INFO: Usually here is a locking script (to check who and when this transaction output can be used), in this case it's a simple destination address     
                            }
                        }
                    }
                };

                transactions.Add(rewardTransaction);
            }

            return new CurrencyBlock
			{
                Index = index,
                Nonce = 0,
                PreviousHash = previousHash,
                Timestamp = timestamp,
                Transactions = transactions
            };
        }
    }
}