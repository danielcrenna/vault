using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChainLib.Models
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
	    IEnumerable<T> StreamAllBlocks(bool forwards = true, long startingAt = 0);
		IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards = true, long startingAt = 0);
	    IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingFrom);
    }
}