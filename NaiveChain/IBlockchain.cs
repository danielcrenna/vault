using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveChain
{
	public interface IBlockchain<T> where T : Block
	{
		void Init();

		Task<T> GetBlockByIndexAsync(long index);
		Task<T> GetBlockByHashAsync(string hash);
		Task<T> GetLastBlockAsync();
		Task ReplaceChainAsync(List<T> newBlockchain);
		Task<T> AddBlockAsync(T block);
		
		bool CheckChain(IReadOnlyList<T> blockchainToValidate);
		bool CheckBlock(T newBlock, T previousBlock);
		
		IEnumerable<T> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();
	}
}