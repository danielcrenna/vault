using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NaiveCoin.Models;
using NaiveCoin.Models.Exceptions;
using Newtonsoft.Json;
using NaiveCoin.Core.Providers;

namespace NaiveCoin.Services
{
    public class Blockchain
    {
        private readonly IBlockRepository _blocks;
        private readonly ITransactionRepository _transactions;
        private readonly IObjectHashProvider _hashProvider;
        private readonly ILogger _logger;

        private readonly CoinSettings _coinSettings;
        private readonly JsonSerializerSettings _jsonSettings;

        public Blockchain(CoinSettings coinSettings, IBlockRepository blocks, ITransactionRepository transactions, IObjectHashProvider hashProvider, JsonSerializerSettings jsonSettings, ILogger<Blockchain> logger)
        {
            _coinSettings = coinSettings;
            _blocks = blocks;
            _transactions = transactions;
            _hashProvider = hashProvider;
            _logger = logger;
            _jsonSettings = jsonSettings;

            Init();
        }

        public void Init()
        {
            // Create the genesis block if the blockchain is empty
            if (_blocks.GetLength() == 0)
            {
                foreach (var transaction in _coinSettings.GenesisBlock.Transactions)
                    transaction.Hash = transaction.ToHash(_hashProvider);
                _coinSettings.GenesisBlock.Hash = _coinSettings.GenesisBlock.ToHash(_hashProvider);

                _blocks.Add(_coinSettings.GenesisBlock);
            }

            // Remove transactions that are in the blockchain
            _transactions.Delete(_blocks.GetAllTransactionIds());
        }

        public IEnumerable<Block> GetAllBlocks()
        {
            return _blocks.GetAll();
        }

        public Block GetBlockByIndex(long index)
        {
            return _blocks.GetByIndex(index);
        }

        public Block GetBlockByHash(string hash)
        {
            return _blocks.GetByHash(hash);
        }
        
        public Block GetLastBlock()
        {
            return _blocks.GetLastBlock();
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

        public IEnumerable<Transaction> GetAllTransactions()
        {
            return _transactions.GetAll();
        }

        public Transaction GetTransactionById(string id)
        {
            return _transactions.GetById(id);
        }

        public Transaction GetTransactionFromBlocks(string transactionId)
        {
            return _blocks.GetByTransactionId(transactionId)?.Transactions.SingleOrDefault(x => x.Id == transactionId);
        }

        public void ReplaceChain(List<Block> newBlockchain)
        {
            // It doesn't make sense to replace this blockchain by a smaller one
            if (newBlockchain.Count <= _blocks.GetLength())
            {
                var message = $"Blockchain shorter than the current blockchain";
                _logger?.LogError(message);
                throw new BlockchainAssertionException(message);
            }

            // Verify if the new blockchain is correct
            CheckChain(newBlockchain);

            // Get the blocks that diverge from our blockchain
            _logger?.LogInformation($"Received blockchain is valid. Replacing current blockchain with received blockchain");

            // Get the blocks that diverge from our blockchain
            var start = (int)(newBlockchain.Count - _blocks.GetLength());
            foreach (var block in newBlockchain.Skip(start))
            {
                AddBlock(block);
            }
        }

        private bool CheckChain(IReadOnlyList<Block> blockchainToValidate)
        {
            // Check if the genesis block is the same
            if (_hashProvider.ComputeHash(blockchainToValidate[0]) !=
                _hashProvider.ComputeHash(_coinSettings.GenesisBlock))
            {
                var message = $"Genesis blocks aren't the same";
                _logger?.LogError(message);
                throw new BlockchainAssertionException(message);
            }

            // Compare every block to the previous one (it skips the first one, because it was verified before)
            try
            {
                for (var i = 1; i < blockchainToValidate.Count; i++)
                {
                    CheckBlock(blockchainToValidate[i], blockchainToValidate[i - 1]);
                }
            }
            catch (Exception ex)
            {
                var message = $"Invalid block sequence";
                _logger?.LogError(message);
                throw new BlockchainAssertionException(message, ex);
            }

            return true;
        }

        public Block AddBlock(Block block)
        {
            // It only adds the block if it's valid (we need to compare to the previous one)
            if (CheckBlock(block, GetLastBlock()))
            {
                _blocks.Add(block);

                // After adding the block it removes the transactions of this block from the list of pending transactions
                RemoveBlockTransactionsFromTransactions(block);

                _logger?.LogInformation($"Block added: {block.Hash}");
                _logger?.LogDebug($"Block added: {JsonConvert.SerializeObject(block)}");
                
                return block;
            }

            return null;
        }

        public Transaction AddTransaction(Transaction transaction)
        {
            // It only adds the transaction if it's valid
            if (!CheckTransaction(transaction))
                return null;

            _transactions.Add(transaction);
            _logger?.LogInformation($"Transaction added: {transaction.Id}");
            _logger?.LogDebug($"Transaction added: {JsonConvert.SerializeObject(transaction, _jsonSettings)}");

            return transaction;
        }

        private void RemoveBlockTransactionsFromTransactions(Block block)
        {
            _transactions.Delete(block.Transactions.Select(x => x.Id));
        }

        private bool CheckBlock(Block newBlock, Block previousBlock)
        {
            var blockHash = newBlock.ToHash(_hashProvider);

            if (previousBlock.Index + 1 != newBlock.Index)
            { // Check if the block is the last one
                var message = $"Invalid index: expected '{previousBlock.Index + 1}' but got '{newBlock.Index}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }
            else if (previousBlock.Hash != newBlock.PreviousHash)
            { // Check if the previous block is correct
                var message = $"Invalid previoushash: expected '{previousBlock.Hash}' got '{newBlock.PreviousHash}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }
            else if (blockHash != newBlock.Hash)
            { // Check if the hash is correct
                var message = $"Invalid hash: expected '{blockHash}' got '{newBlock.Hash}'";
                throw new BlockAssertionException(message);
            }
            else if (newBlock.GetDifficulty() >= GetDifficulty(newBlock.Index ?? 0))
            { // If the difficulty level of the proof-of-work challenge is correct
                var message = $"Invalid proof-of-work difficulty: expected '${newBlock.GetDifficulty()}' to be smaller than '${GetDifficulty(newBlock.Index ?? 0)}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            // For each transaction in this block, check if it is valid
            foreach (var transaction in newBlock.Transactions)
                CheckTransaction(transaction);

            // Check if the sum of output transactions are equal the sum of input transactions + the reward for the block miner

            var sumOfInputsAmount = newBlock.Transactions.SelectMany(x => x.Data.Inputs).Select(x => x.Amount).Sum();
            var sumOfOutputsAmount = newBlock.Transactions.SelectMany(x => x.Data.Outputs).Select(x => x.Amount).Sum();

            if (sumOfInputsAmount < sumOfOutputsAmount)
            {
                var message = $"Invalid block balance: inputs sum '{sumOfInputsAmount}', outputs sum '{sumOfOutputsAmount}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            // Check if there is only 1 fee transaction and 1 reward transaction;
            var feeTransactions = newBlock.Transactions.Count(x => x.Type == TransactionType.Fee);
            if (feeTransactions > 1)
            {
                var message = $"Invalid fee transaction count: expected '1' got '${feeTransactions}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }
            var rewardTransactions = newBlock.Transactions.Count(x => x.Type == TransactionType.Reward);
            if (rewardTransactions > 1)
            {
                var message = $"Invalid reward transaction count: expected '1' got '${rewardTransactions}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }

            return true;
        }

        public bool CheckTransaction(Transaction transaction)
        {
            // Check the transaction
            transaction.Check(_hashProvider, _coinSettings);

            // Verify if the transaction isn't already in the blockchain
            if (_blocks.GetByTransactionId(transaction.Id) != null)
            {
                var message = $"Transaction '{transaction.Id}' is already in the blockchain";
                _logger?.LogError(message);
                throw new TransactionAssertionException(message);
            }

            // Verify if all input transactions are unspent in the blockchain
            var blockInputs = _blocks.GetAll().SelectMany(x => x.Transactions).SelectMany(x => x.Data.Inputs);
            var txInputs = transaction.Data.Inputs.Zip(blockInputs, (ti, i) => ti.Index == i.Index && ti.TransactionId == i.TransactionId);
            var isInputTransactionsUnspent = txInputs.All(x => false);
            if (!isInputTransactionsUnspent)
            {
                var message = $"Not all inputs are unspent for transaction '{transaction.Id}'";
                _logger?.LogError(message);
                throw new TransactionAssertionException(message);
            }

            return true;
        }

        public IEnumerable<TransactionOutput> GetUnspentTransactionsForAddress(string address)
        {
            // Create a list of all transactions outputs found for an address (or all).
            IEnumerable<TransactionOutput> outputs = _blocks.GetTransactionOutputsForAddress(address);

            // Create a list of all transactions inputs found for an address (or all).
            IEnumerable<TransactionInput> inputs = _blocks.GetTransactionInputsForAddress(address);

            // Cross both lists and find transactions outputs without a corresponding transaction input
            var unspent = new HashSet<TransactionOutput>();
            foreach (var output in outputs)
            {
                if (!inputs.Any(x => x.TransactionId == output.TransactionId && x.Index == output.Index))
                {
                    unspent.Add(output);
                }
            }

            return unspent;
        }
    }
}