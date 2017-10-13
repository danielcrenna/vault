using NaiveChain.Models;
using NaiveChain.Serialization;
using NaiveCoin.Models.Exceptions;

namespace NaiveCoin.Models
{
    public class Transaction : IBlockSerialized
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public TransactionType Type { get; set; }
        public TransactionData Data { get; set; }

        public string ToHash(IHashProvider provider)
        {
            return provider.ComputeHashString($"{Id}{Type}{provider.ComputeHashString(Data)}");
        }

        public void Check(IHashProvider hashProvider, CoinSettings coinSettings)
        {
            // Check if the transaction hash is correct
            if (Hash != ToHash(hashProvider))
                throw new TransactionAssertionException($"Invalid transaction hash '{Hash}'");

            Data.Check(coinSettings, hashProvider);
        }
		
	    public void Serialize(BlockSerializeContext context)
	    {
		    throw new System.NotImplementedException();
	    }

	    public object Deserialize(BlockDeserializeContext context)
	    {
		    throw new System.NotImplementedException();
	    }
    }
}