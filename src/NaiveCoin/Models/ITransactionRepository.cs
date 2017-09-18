using System.Collections.Generic;

namespace NaiveCoin.Models
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> GetAll();
        Transaction GetById(string id);
        void Add(Transaction transaction);
        int Delete(IEnumerable<string> ids);
        bool Delete(string id);
    }
}