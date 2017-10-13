using System;
using NaiveChain.Serialization;

namespace NaiveChain.Models
{
	public interface IBlockObjectTypeProvider
	{
		void Add(long id, Type type);
		long? Get(Type type);
		Type Get(long typeId);
		IBlockSerialized Deserialize(Type type, BlockDeserializeContext context);
	}
}