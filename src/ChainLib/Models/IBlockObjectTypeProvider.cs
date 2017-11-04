using System;
using ChainLib.Serialization;

namespace ChainLib.Models
{
	public interface IBlockObjectTypeProvider
	{
		void Add(long id, Type type);
		long? Get(Type type);
		Type Get(long typeId);
		IBlockSerialized Deserialize(Type type, BlockDeserializeContext context);
	}
}