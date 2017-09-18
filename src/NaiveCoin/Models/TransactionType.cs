namespace NaiveCoin.Models
{
    public enum TransactionType : byte
    {
        Unknown,
        Regular,
        Fee,
        Reward
    }
}