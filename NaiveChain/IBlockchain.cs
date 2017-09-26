using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveChain
{
	public interface IBlockchain<T> where T : Block
	{
		void Init();
		IEnumerable<T> StreamAllBlocks();
		Task<T> GetBlockByIndexAsync(long index);
		Task<T> GetBlockByHashAsync(string hash);
		Task<T> GetLastBlockAsync();
		Task ReplaceChainAsync(List<T> newBlockchain);
		bool CheckChain(IReadOnlyList<T> blockchainToValidate);
		Task<T> AddBlockAsync(T block);
		bool CheckBlock(T newBlock, T previousBlock);
	}
}