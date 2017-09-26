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
        Task<IEnumerable<TransactionItem>> GetTransactionItemsForAddressAsync(TransactionDataType type, string address);

        void Add(Block block);

	    IEnumerable<string> StreamAllTransactionIds();
	    IEnumerable<Block> StreamAllBlocks();
		IEnumerable<BlockObject> StreamAllBlockObjects();
    }
}