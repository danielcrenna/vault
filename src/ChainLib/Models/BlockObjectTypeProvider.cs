using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ChainLib.Serialization;

namespace ChainLib.Models
{
	public class BlockObjectTypeProvider : IBlockObjectTypeProvider
	{
		public byte[] SecretKey { get; }

		public BlockObjectTypeProvider(byte[] secretKey = null)
		{
			SecretKey = secretKey;
			Map = new ConcurrentDictionary<long, Type>();
			ReverseMap = new ConcurrentDictionary<Type, long>();
			Serializers = new ConcurrentDictionary<Type, ConstructorInfo>();
		}
		
		public IDictionary<long, Type> Map { get; }
		public IDictionary<Type, long> ReverseMap { get; }
		public IDictionary<Type, ConstructorInfo> Serializers { get; }

		public bool TryAdd(long id, Type type)
		{
			Debug.Assert(typeof(IBlockSerialized).IsAssignableFrom(type));

			if (!Map.TryGetValue(id, out Type added))
			{
				Map.Add(id, type);
				ReverseMap.Add(type, id);
				Serializers.Add(type, type.GetConstructor(new[] { typeof(BlockDeserializeContext) }));
				return true;
			}
			return false;
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