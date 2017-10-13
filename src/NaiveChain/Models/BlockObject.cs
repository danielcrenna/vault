using NaiveChain.Serialization;

namespace NaiveChain.Models
{
	public class BlockObject : IBlockSerialized
	{
		public BlockObject() { }

		public int Index { get; set; }
		public long Version { get; set; }
		public IBlockSerialized Data { get; set; }
		public long Timestamp { get; set; }
		public byte[] Hash { get; set; }

		#region Serialization

		public object Deserialize(BlockDeserializeContext context)
		{
			return new BlockObject(context);
		}

		public void Serialize(BlockSerializeContext context)
		{
			var typeId = context.typeProvider.Get(Data?.GetType());

			context.bw.WriteNullableLong(typeId);    // TypeId
			context.bw.Write(Version);               // Version
			context.bw.Write(Timestamp);             // Timestamp
			context.bw.WriteBuffer(Hash);            // Hash

			if (context.bw.WriteBoolean(Data != null) && typeId.HasValue)
				Data?.Serialize(context);
		}
		
		public BlockObject(BlockDeserializeContext context)
		{
			var typeId = context.br.ReadNullableLong(); // TypeId
			Version = context.br.ReadInt64();			// Version
			Timestamp = context.br.ReadInt64();			// Timestamp
			Hash = context.br.ReadBuffer();				// Hash

			if (context.br.ReadBoolean() && typeId.HasValue)
			{
				var type = context.typeProvider.Get(typeId.Value);
				if (type != null)
					Data = context.typeProvider.Deserialize(type, context);
			}
		}

		#endregion
	}
}