namespace NaiveCoin.Models
{
    public interface ITransactionDataSerializer
    {
        byte[] Serialize(TransactionData transactionData);
        TransactionData Deserialize(byte[] json);
    }
}