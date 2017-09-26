using System.Collections.Generic;
using System.Threading.Tasks;
using NaiveChain;

namespace NaiveCoin.Models
{
	public interface ICurrencyBlockRepository : IBlockRepository<CurrencyBlock>
	{
		Task<CurrencyBlock> GetByTransactionIdAsync(string transactionId);
		Task<IEnumerable<TransactionItem>> GetTransactionItemsForAddressAsync(TransactionDataType type, string address);
		IEnumerable<string> StreamAllTransactionIds();
	}
}