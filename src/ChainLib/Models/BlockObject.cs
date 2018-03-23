using ChainLib.Serialization;

namespace ChainLib.Models
{
	public class BlockObject : IBlockSerialized
	{
		public BlockObject() { }

		public long? Type { get; set; }
		public int Index { get; set; }
		public long Version { get; set; }
		public IBlockSerialized Data { get; set; }
		public long Timestamp { get; set; }

		[Computed]
		public byte[] Hash { get; set; }

		#region Serialization

		public object Deserialize(BlockDeserializeContext context)
		{
			return new BlockObject(context);
		}

		public void Serialize(BlockSerializeContext context)
		{
			Type = context.typeProvider.Get(Data?.GetType());

			context.bw.WriteNullableLong(Type);		 // Type
			context.bw.Write(Version);               // Version
			context.bw.Write(Timestamp);             // Timestamp
			context.bw.WriteBuffer(Hash);            // Hash

			if (context.bw.WriteBoolean(Data != null) && Type.HasValue)
				Data?.Serialize(context);
		}
		
		public BlockObject(BlockDeserializeContext context)
		{
			Type = context.br.ReadNullableLong();		// Type
			Version = context.br.ReadInt64();			// Version
			Timestamp = context.br.ReadInt64();			// Timestamp
			Hash = context.br.ReadBuffer();				// Hash

			if (context.br.ReadBoolean() && Type.HasValue)
			{
				var type = context.typeProvider.Get(Type.Value);
				if (type != null)
					Data = context.typeProvider.Deserialize(type, context);
			}
		}

		#endregion
	}
}