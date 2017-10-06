using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NaiveChain.Exceptions;
using NaiveChain.Extensions;
using NaiveChain.Models;
using NaiveCoin.Core;
using Newtonsoft.Json;

namespace NaiveChain.Services
{
	public class Blockchain : IBlockchain<Block>
	{
		private readonly IBlockRepository<Block> _blocks;
		private readonly IProofOfWork _proofOfWork;
		private readonly IHashProvider _hashProvider;
		private readonly ILogger _logger;

		public Blockchain(IBlockRepository<Block> blocks, IProofOfWork proofOfWork, IHashProvider hashProvider, ILogger<Blockchain> logger)
		{
			_blocks = blocks;
			_hashProvider = hashProvider;
			_logger = logger;
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
		}

		public IEnumerable<Block> StreamAllBlocks()
		{
			return _blocks.StreamAllBlocks();
		}

		public IEnumerable<BlockObject> StreamAllBlockObjects()
		{
			return _blocks.StreamAllBlockObjects();
		}

		public async Task<Block> GetBlockByIndexAsync(long index)
		{
			return await _blocks.GetByIndexAsync(index);
		}

		public async Task<Block> GetBlockByHashAsync(string hash)
		{
			return await _blocks.GetByHashAsync(hash);
		}

		public async Task<Block> GetLastBlockAsync()
		{
			return await _blocks.GetLastBlockAsync();
		}

		public double GetDifficulty(long index)
		{
			return _proofOfWork.GetDifficulty(index);
		}

		public async Task ReplaceChainAsync(List<Block> newBlockchain)
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

		public async Task<bool> CheckChainAsync(IReadOnlyList<Block> blockchainToValidate)
		{
			// Check if the genesis block is the same
			if (_hashProvider.ComputeHash(blockchainToValidate[0]) !=
				_hashProvider.ComputeHash(await _blocks.GetGenesisBlockAsync()))
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

		public async Task<Block> AddBlockAsync(Block block)
		{
			// It only adds the block if it's valid (we need to compare to the previous one)
			if (true || CheckBlock(block, await GetLastBlockAsync()))
			{
				await _blocks.AddAsync(block);

				_logger?.LogInformation($"Block added: {block.Hash}");
				_logger?.LogDebug($"Block added: {JsonConvert.SerializeObject(block)}");

				return block;
			}

			return null;
		}

		public bool CheckBlock(Block newBlock, Block previousBlock)
		{
			var blockHash = newBlock.ToHash(_hashProvider);

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
				var message = $"Invalid proof-of-work difficulty: expected '${newBlock.GetDifficulty()}' to be smaller than '${GetDifficulty(newBlock.Index ?? 0)}'";
				_logger?.LogError(message);
				throw new BlockAssertionException(message);
			}

			return true;
		}

		IEnumerable<BlockObject> IBlockchain<Block>.StreamAllBlockObjects()
		{
			throw new NotImplementedException();
		}
	}
}