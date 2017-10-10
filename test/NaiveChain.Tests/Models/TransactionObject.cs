using NaiveChain.Models;
using NaiveChain.Serialization;

namespace NaiveChain.Tests.Models
{
    public class TransactionObject : IBlockSerialized
    {
		public enum TransactionType : byte { Unknown, Regular, Fee, Reward }

		public string Id { get; set; }
	    public string Hash { get; set; }
	    public TransactionType Type { get; set; }
	    public IBlockSerialized Data { get; set; }

	    public string ToHashString(IHashProvider provider)
	    {
		    return provider.ComputeHash($"{Id}{Type}{provider.ComputeHash(Data)}");
	    }

	    public uint ObjectType => (uint) typeof(TransactionObject).MetadataToken;

	    public void Serialize(BlockSerializeContext context)
	    {
		    context.bw.Write(Id);
		    context.bw.Write(Hash);
		    context.bw.Write((byte)Type);
			Data?.Serialize(context);
		}

	    public object Deserialize(BlockDeserializeContext context)
	    {
		    throw new System.NotImplementedException();
	    }
    }
}
