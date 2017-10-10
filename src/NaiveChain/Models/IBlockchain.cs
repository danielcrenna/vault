using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveChain.Models
{
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
		
		IEnumerable<T> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();

		double GetDifficulty(long index);
	}
}