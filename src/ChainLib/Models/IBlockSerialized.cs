using ChainLib.Serialization;

namespace ChainLib.Models
{
	public interface IBlockSerialized
	{
		void Serialize(BlockSerializeContext context);
	}
}