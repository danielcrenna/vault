using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChainLib.Exceptions;
using ChainLib.Extensions;
using ChainLib.Models;
using Newtonsoft.Json;

namespace ChainLib.Services
{
	public class Blockchain : IBlockchain
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

		public IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingAt = 0)
		{
			return _blocks.StreamAllBlockHeaders(forwards, startingAt);
		}

		public IEnumerable<Block> StreamAllBlocks(bool forwards, long startingAt = 0)
		{
			return _blocks.StreamAllBlocks(forwards, startingAt);
		}

		public IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, long startingAt = 0)
		{
			return _blocks.StreamAllBlockObjects(forwards, startingAt);
		}

		public async Task<Block> GetBlockByIndexAsync(long index)
		{
			return await _blocks.GetByIndexAsync(index);
		}

		public async Task<Block> GetBlockByHashAsync(byte[] hash)
		{
			return await _blocks.GetByHashAsync(hash);
		}

		public async Task<Block> GetLastBlockAsync()
		{
			return await _blocks.GetLastBlockAsync();
		}

		public uint GetDifficulty(long index)
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
			await ChainIsValid(newBlockchain);

			// Get the blocks that diverge from our blockchain
			_logger?.LogInformation($"Received blockchain is valid. Replacing current blockchain with received blockchain");

			// Get the blocks that diverge from our blockchain
			var start = (int)(newBlockchain.Count - await _blocks.GetLengthAsync());
			foreach (var block in newBlockchain.Skip(start))
			{
				await AddBlockAsync(block);
			}
		}

		public async Task<bool> ChainIsValid(IReadOnlyList<Block> blockchainToValidate)
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
					BlockIsValid(blockchainToValidate[i], blockchainToValidate[i - 1]);
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
			if (BlockIsValid(block, await GetLastBlockAsync()))
			{
				await _blocks.AddAsync(block);

				_logger?.LogInformation($"Block added: {block.Hash}");
				_logger?.LogDebug($"Block added: {JsonConvert.SerializeObject(block)}");

				return block;
			}

			return null;
		}

		public bool BlockIsValid(Block newBlock, Block previousBlock)
		{
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

			var blockHash = newBlock.ToHashBytes(_hashProvider);
			if (blockHash != newBlock.Hash)
			{ // Check if the hash is correct
				var message = $"Invalid hash: expected '{blockHash}' got '{newBlock.Hash}'";
				throw new BlockAssertionException(message);
			}

			var merkleRootHash = newBlock.ComputeMerkleRoot(_hashProvider);
			if (merkleRootHash != newBlock.MerkleRootHash)
			{
				var message = $"Invalid merkle root: expected '{merkleRootHash}' got '{newBlock.MerkleRootHash}'";
				throw new BlockAssertionException(message);
			}

			if (newBlock.Difficulty >= GetDifficulty(newBlock.Index ?? 0))
			{ // If the difficulty level of the proof-of-work challenge is correct
				var message = $"Invalid difficulty: expected '${newBlock.Difficulty}' to be smaller than '${GetDifficulty(newBlock.Index ?? 0)}'";
				_logger?.LogError(message);
				throw new BlockAssertionException(message);
			}

			return true;
		}
	}
}