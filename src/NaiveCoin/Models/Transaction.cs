using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Providers;
using NaiveCoin.Models.Exceptions;

namespace NaiveCoin.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public TransactionType Type { get; set; }
        public TransactionData Data { get; set; }

        public string ToHash(IHashProvider provider)
        {
            return provider.ComputeHash($"{Id}{Type}{provider.ComputeHash(Data)}");
        }

        public void Check(IHashProvider hashProvider, CoinSettings coinSettings)
        {
            // Check if the transaction hash is correct
            if (Hash != ToHash(hashProvider))
                throw new TransactionAssertionException($"Invalid transaction hash '{Hash}'");

            Data.Check(coinSettings, hashProvider);
        }
    }
}