using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaiveCoin.Models
{
    public interface IBlockRepository
    {
        Task<long> GetLengthAsync();
        Task<Block> GetByIndexAsync(long index);
	    Task<Block> GetByHashAsync(string hash);
        Task<Block> GetLastBlockAsync();
        Task<Block> GetByTransactionIdAsync(string transactionId);

        void Add(Block block);

        IEnumerable<TransactionItem> GetTransactionItemsForAddress(TransactionDataType type, string address);
        IEnumerable<string> GetAllTransactionIds();

	    IEnumerable<Block> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();
    }
}