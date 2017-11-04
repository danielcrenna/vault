using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ChainLib.Serialization;

namespace ChainLib.Models
{
	public class BlockObjectTypeProvider : IBlockObjectTypeProvider
	{
		public BlockObjectTypeProvider()
		{
			Map = new Dictionary<long, Type>();
			ReverseMap = new Dictionary<Type, long>();
			Serializers = new Dictionary<Type, ConstructorInfo>();
		}
		
		public Dictionary<long, Type> Map { get; }
		public Dictionary<Type, long> ReverseMap { get; }
		public Dictionary<Type, ConstructorInfo> Serializers { get; }

		public void Add(long id, Type type)
		{
			Debug.Assert(typeof(IBlockSerialized).IsAssignableFrom(type));
			Map.Add(id, type);
			ReverseMap.Add(type, id);
			Serializers.Add(type, type.GetConstructor(new[] { typeof(BlockDeserializeContext) }));
		}
		
		public long? Get(Type type)
		{
			return !ReverseMap.TryGetValue(type, out var result) ? (long?) null : result;
		}

		public Type Get(long typeId)
		{
			return !Map.TryGetValue(typeId, out var result) ? null : result;
		}

		public IBlockSerialized Deserialize(Type type, BlockDeserializeContext context)
		{
			if (!Serializers.TryGetValue(type, out var serializer))
				return null;
			var deserialized = serializer.Invoke(new object[] { context });
			return (IBlockSerialized) deserialized;
		}
	}
}