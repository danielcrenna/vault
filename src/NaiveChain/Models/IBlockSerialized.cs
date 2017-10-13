using NaiveChain.Serialization;

namespace NaiveChain.Models
{
	public interface IBlockSerialized
	{
		void Serialize(BlockSerializeContext context);
	}
}