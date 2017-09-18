namespace NaiveCoin.Models
{
    public class TransactionInput
    {
        public string TransactionId { get; set; }
        public long Index { get; set; }
        public long Amount { get; set; }
        public byte[] Address { get; set; }
        public byte[] Signature { get; set; }
    }
}