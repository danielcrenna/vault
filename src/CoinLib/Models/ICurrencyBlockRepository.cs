using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Models;
using CoinLib.Models;

namespace CoinLib.Models
{
	public interface ICurrencyBlockRepository : IBlockRepository<CurrencyBlock>
	{
		Task<CurrencyBlock> GetByTransactionIdAsync(string transactionId);
		Task<IEnumerable<TransactionItem>> GetTransactionItemsForAddressAsync(TransactionDataType type, string address);
		IEnumerable<string> StreamAllTransactionIds();
	}
}