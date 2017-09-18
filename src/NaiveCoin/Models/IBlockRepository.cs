using System.Collections.Generic;

namespace NaiveCoin.Models
{
    public interface IBlockRepository
    {
        long GetLength();

        IEnumerable<Block> GetAll();
        Block GetByIndex(long index);
        Block GetByHash(string hash);
        Block GetLastBlock();
        Block GetByTransactionId(string transactionId);

        void Add(Block block);

        IEnumerable<TransactionInput> GetTransactionInputsForAddress(string address);
        IEnumerable<TransactionOutput> GetTransactionOutputsForAddress(string address);
        
        IEnumerable<string> GetAllTransactionIds();
    }
}