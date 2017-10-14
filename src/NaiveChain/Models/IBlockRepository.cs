using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveChain.Models
{
	public interface IBlockRepository : IBlockRepository<Block> { }

    public interface IBlockRepository<T> where T : Block
    {
	    Task<T> GetGenesisBlockAsync();
		Task<long> GetLengthAsync();
        Task<T> GetByIndexAsync(long index);
	    Task<T> GetByHashAsync(byte[] hash);
        Task<T> GetLastBlockAsync();
        Task AddAsync(T block);
	    IEnumerable<T> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();
    }
}