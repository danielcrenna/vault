using NaiveChain.Serialization;

namespace NaiveChain.Models
{
	public interface IBlockSerialized
	{
		uint ObjectType { get; }
		void Serialize(BlockSerializeContext context);
		object Deserialize(BlockDeserializeContext context);
	}
}