using NaiveChain.Serialization;

namespace NaiveChain.Models
{
	public class BlockObject : IBlockSerialized
	{
		public BlockObject() { }

		public int Index { get; set; }
		public string SourceId { get; set; }
		public long Version { get; set; }
		public IBlockSerialized Data { get; set; }
		public long Timestamp { get; set; }
		public byte[] Hash { get; set; }

		#region Serialization

		public uint ObjectType => 0;

		public object Deserialize(BlockDeserializeContext context)
		{
			return new BlockObject(context);
		}

		public void Serialize(BlockSerializeContext context)
		{
			SerializeHeader(context);

			if (context.bw.WriteBoolean(Data != null))
			{
				context.bw.Write(ObjectType);
				Data?.Serialize(context);
			}
		}
		
		private void SerializeHeader(BlockSerializeContext context)
		{
			context.bw.Write(SourceId);
			context.bw.Write(Version);
			context.bw.Write(Timestamp);
			context.bw.WriteBuffer(Hash);
		}

		public BlockObject(BlockDeserializeContext context)
		{
			DeserializeHeader(context);
			context.br.ReadUInt32();		// Type
		}

		private void DeserializeHeader(BlockDeserializeContext context)
		{
			SourceId = context.br.ReadString();
			Version = context.br.ReadInt64();
			Timestamp = context.br.ReadInt64();
			Hash = context.br.ReadBuffer();
		}

		#endregion
	}
}