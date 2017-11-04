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
		
		Task<bool> CheckChainAsync(IReadOnlyList<T> blockchainToValidate);
		bool CheckBlock(T newBlock, T previousBlock);

		IEnumerable<T> StreamAllBlocks(bool forwards, int startingAt = 0);
		IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, int startingAt = 0);

		double GetDifficulty(long index);
	}
}