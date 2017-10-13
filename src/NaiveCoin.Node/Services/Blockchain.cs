using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveChain.Exceptions;
using NaiveChain.Models;
using NaiveCoin.Extensions;
using NaiveCoin.Models;
using NaiveCoin.Models.Exceptions;
using Newtonsoft.Json;

namespace NaiveCoin.Node.Services
{
	public class Blockchain : ICurrencyBlockchain
	{
        private readonly ICurrencyBlockRepository _blocks;
        private readonly IProofOfWork _proofOfWork;
        private readonly ITransactionRepository _transactions;
        private readonly IHashProvider _hashProvider;
        private readonly ILogger _logger;

        private readonly CoinSettings _coinSettings;
        private readonly JsonSerializerSettings _jsonSettings;

        public Blockchain(IOptions<CoinSettings> coinSettings, ICurrencyBlockRepository blocks, IProofOfWork proofOfWork, ITransactionRepository transactions, IHashProvider hashProvider, JsonSerializerSettings jsonSettings, ILogger<Blockchain> logger)
        {
            _coinSettings = coinSettings.Value;
            _blocks = blocks;
            _transactions = transactions;
            _hashProvider = hashProvider;
            _logger = logger;
            _jsonSettings = jsonSettings;
            _proofOfWork = proofOfWork;

            Init();
        }

        public void Init()
        {
            // Create the genesis block if the blockchain is empty
	        var height = _blocks.GetLengthAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	        if (height == 0)
            {
				var genesisBlock = _blocks.GetGenesisBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				_blocks.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            // Remove transactions that are in the blockchain
            _transactions.DeleteAsync(_blocks.StreamAllTransactionIds()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public IEnumerable<CurrencyBlock> StreamAllBlocks()
        {
            return _blocks.StreamAllBlocks();
        }

		public IEnumerable<BlockObject> StreamAllBlockObjects()
		{
			return _blocks.StreamAllBlockObjects();
		}

		public async Task<CurrencyBlock> GetBlockByIndexAsync(long index)
        {
            return await _blocks.GetByIndexAsync(index);
        }

        public async Task<CurrencyBlock> GetBlockByHashAsync(byte[] hash)
        {
            return await _blocks.GetByHashAsync(hash);
        }
        
        public async Task<CurrencyBlock> GetLastBlockAsync()
        {
            return await _blocks.GetLastBlockAsync();
        }

        public double GetDifficulty(long index)
        {
            return _proofOfWork.GetDifficulty(index);
        }

        public IEnumerable<Transaction> StreamAllTransactions()
        {
            return _transactions.StreamAllTransactions();
        }

        public async Task<Transaction> GetTransactionByIdAsync(string id)
        {
            return await _transactions.GetByIdAsync(id);
        }

        public async Task<Transaction> GetTransactionFromBlocksAsync(string transactionId)
        {
	        var blocks = await _blocks.GetByTransactionIdAsync(transactionId);

			return blocks?.Transactions.SingleOrDefault(x => x.Id == transactionId);
        }

        public async Task ReplaceChainAsync(List<CurrencyBlock> newBlockchain)
        {
            // It doesn't make sense to replace this blockchain by a smaller one
            if (newBlockchain.Count <= await _blocks.GetLengthAsync())
            {
                var message = $"Blockchain shorter than the current blockchain";
                _logger?.LogError(message);
                throw new BlockchainAssertionException(message);
            }

            // Verify if the new blockchain is correct
            await CheckChainAsync(newBlockchain);

            // Get the blocks that diverge from our blockchain
            _logger?.LogInformation($"Received blockchain is valid. Replacing current blockchain with received blockchain");

            // Get the blocks that diverge from our blockchain
            var start = (int)(newBlockchain.Count - await _blocks.GetLengthAsync());
            foreach (var block in newBlockchain.Skip(start))
            {
                await AddBlockAsync(block);
            }
        }

		public async Task<bool> CheckChainAsync(IReadOnlyList<CurrencyBlock> blockchainToValidate)
        {
            // Check if the genesis block is the same
            if (_hashProvider.ComputeHashString(blockchainToValidate[0]) !=
                _hashProvider.ComputeHashString(await _blocks.GetGenesisBlockAsync()))
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

        public async Task<CurrencyBlock> AddBlockAsync(CurrencyBlock block)
        {
	        var previousBlock = await GetLastBlockAsync();
			
			Contract.Assert(block != null);
	        Contract.Assert(previousBlock != null);

			// It only adds the block if it's valid (we need to compare to the previous one)
			if (true || CheckBlock(block, previousBlock))
            {
	            await _blocks.AddAsync(block);

				// After adding the block it removes the transactions of this block from the list of pending transactions
				await _transactions.DeleteAsync(block.Transactions.Select(x => x.Id));

				_logger?.LogInformation($"Block added: {block.Hash}");
                _logger?.LogDebug($"Block added: {JsonConvert.SerializeObject(block)}");
                
                return block;
            }

            return null;
        }
		
		public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            // It only adds the transaction if it's valid
            //if (!CheckTransaction(transaction))
            //    return null;

            await _transactions.AddTransactionAsync(transaction);
            _logger?.LogInformation($"Transaction added: {transaction.Id}");
            _logger?.LogDebug($"Transaction added: {JsonConvert.SerializeObject(transaction, _jsonSettings)}");

            return transaction;
        }
		
		public bool CheckBlock(CurrencyBlock newBlock, CurrencyBlock previousBlock)
		{
			var blockHash = newBlock.ToHashBytes(_hashProvider);

            if (previousBlock.Index + 1 != newBlock.Index)
            { // Check if the block is the last one
                var message = $"Invalid index: expected '{previousBlock.Index + 1}' but got '{newBlock.Index}'";
                _logger?.LogError(message);
                throw new BlockAssertionException(message);
            }
	        if (previousBlock.Hash != newBlock.PreviousHash)
	        { // Check if the previous block is correct
		        var message = $"Invalid previoushash: expected '{previousBlock.Hash}' got '{newBlock.PreviousHash}'";
		        _logger?.LogError(message);
		        throw new BlockAssertionException(message);
	        }
	        if (blockHash != newBlock.Hash)
	        { // Check if the hash is correct
		        var message = $"Invalid hash: expected '{blockHash}' got '{newBlock.Hash}'";
		        throw new BlockAssertionException(message);
	        }
			if (newBlock.GetDifficulty() >= GetDifficulty(newBlock.Index ?? 0))
			{ // If the difficulty level of the proof-of-work challenge is correct
				var message = $"Invalid proof-of-work difficulty: expected '{newBlock.GetDifficulty()}' to be smaller than '{GetDifficulty(newBlock.Index ?? 0)}'";
				_logger?.LogError(message);
				throw new BlockAssertionException(message);
			}

			// For each transaction in this block, check if it is valid
			foreach (var transaction in newBlock.Transactions)
                CheckTransaction(transaction);

			// Check if the sum of output transactions are equal the sum of input transactions + the reward for the block miner
			{
				var sumOfInputsAmount = newBlock.Transactions.SelectMany(x => x.Data.Inputs).Select(x => x.Amount).Sum();
				var sumOfOutputsAmount = newBlock.Transactions.SelectMany(x => x.Data.Outputs).Select(x => x.Amount).Sum();

				if (sumOfInputsAmount < sumOfOutputsAmount)
				{
					var message = $"Invalid block balance: inputs sum '{sumOfInputsAmount}', outputs sum '{sumOfOutputsAmount}'";
					_logger?.LogError(message);
					throw new BlockAssertionException(message);
				}
			}

			// Check if there is only 1 fee transaction and 1 reward transaction
			{
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
			}
           
            return true;
        }

        public bool CheckTransaction(Transaction transaction)
        {
            // Check the transaction
            transaction.Check(_hashProvider, _coinSettings);

            // Verify if the transaction isn't already in the blockchain
            if (_blocks.GetByTransactionIdAsync(transaction.Id) != null)
            {
                var message = $"Transaction '{transaction.Id}' is already in the blockchain";
                _logger?.LogError(message);
                throw new TransactionAssertionException(message);
            }

            // Verify if all input transactions are unspent in the blockchain
			// (this needs serious re-work to use snapshots)
	        var stream = _blocks.StreamAllBlocks();
		    var blockInputs = stream.SelectMany(x => x.Transactions).SelectMany(x => x.Data.Inputs);
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

        public async Task<IEnumerable<TransactionItem>> GetUnspentTransactionsForAddressAsync(string address)
        {
            // Create a list of all transactions outputs found for an address (or all).
            IEnumerable<TransactionItem> outputs = await _blocks.GetTransactionItemsForAddressAsync(TransactionDataType.Output, address);

            // Create a list of all transactions inputs found for an address (or all).
            IEnumerable<TransactionItem> inputs = (await _blocks.GetTransactionItemsForAddressAsync(TransactionDataType.Input, address)).AsList();

            // Cross both lists and find transactions outputs without a corresponding transaction input
            var unspent = new HashSet<TransactionItem>();
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