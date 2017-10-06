using System.Collections.Generic;
using System.Threading.Tasks;
using NaiveChain.Models;

namespace NaiveCoin.Models
{
	public interface ICurrencyBlockchain : IBlockchain<CurrencyBlock>
	{
		IEnumerable<Transaction> StreamAllTransactions();
		Task<Transaction> GetTransactionByIdAsync(string id);
		Task<Transaction> GetTransactionFromBlocksAsync(string transactionId);
		Task<Transaction> AddTransactionAsync(Transaction transaction);
		bool CheckTransaction(Transaction transaction);
		Task<IEnumerable<TransactionItem>> GetUnspentTransactionsForAddressAsync(string address);
	}
}