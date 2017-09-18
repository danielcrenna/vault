using NaiveCoin.Helpers;
using NaiveCoin.Models.Exceptions;

namespace NaiveCoin.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public TransactionType Type { get; set; }
        public TransactionData Data { get; set; }

        public string ToHash(IObjectHashProvider provider)
        {
            return CryptoUtil.Hash($"{Id}{Type}{provider.ComputeHash(Data)}");
        }

        public void Check(IObjectHashProvider provider, CoinSettings coinSettings)
        {
            // Check if the transaction hash is correct
            if (Hash != ToHash(provider))
                throw new TransactionAssertionException($"Invalid transaction hash '{Hash}'");

            Data.Check(coinSettings);
        }
    }
}