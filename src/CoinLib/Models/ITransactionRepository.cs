using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoinLib.Models
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> StreamAllTransactions();
        Task<Transaction> GetByIdAsync(string id);
        Task AddTransactionAsync(Transaction transaction);
        Task<int> DeleteAsync(IEnumerable<string> ids);
        Task<bool> DeleteAsync(string id);
    }
}