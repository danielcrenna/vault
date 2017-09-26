using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveChain
{
    public interface IBlockRepository<T> where T : Block
    {
        Task<long> GetLengthAsync();
        Task<T> GetByIndexAsync(long index);
	    Task<T> GetByHashAsync(string hash);
        Task<T> GetLastBlockAsync();
        void Add(T block);
	    IEnumerable<T> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();
    }
}