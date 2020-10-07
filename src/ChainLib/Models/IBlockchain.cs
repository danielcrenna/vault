using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChainLib.Models
{
	public interface IBlockchain : IBlockchain<Block> { }

	public interface IBlockchain<T> where T : Block
	{
		void Init();

		Task<T> GetBlockByIndexAsync(long index);
		Task<T> GetBlockByHashAsync(byte[] hash);
		Task<T> GetLastBlockAsync();
		Task ReplaceChainAsync(List<T> newBlockchain);
		Task<T> AddBlockAsync(T block);
		
		Task<bool> ChainIsValid(IReadOnlyList<T> blockchainToValidate);
		bool BlockIsValid(T newBlock, T previousBlock);

		IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingAt = 0);
		IEnumerable<T> StreamAllBlocks(bool forwards, long startingAt = 0);
		IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, long startingAt = 0);

		uint GetDifficulty(long index);
	}
}