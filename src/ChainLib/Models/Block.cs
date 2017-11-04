using System;
using System.Collections.Generic;
using System.IO;
using ChainLib.Extensions;
using ChainLib.Serialization;

namespace ChainLib.Models
{
    public class Block
    {
		public Block() { }

		public long? Index { get; set; }
        public byte[] PreviousHash { get; set; }
        public long Timestamp { get; set; }
        public long Nonce { get; set; }
        public ICollection<BlockObject> Objects { get; set; }

		[Computed]
		public byte[] Hash { get; set; }

		public double GetDifficulty()
        {
			return BitConverter.ToDouble(Hash, 24); // take last 8 bytes, not 14 like the original
        }

	    #region Serialization

	    public void Serialize(BlockSerializeContext context)
	    {
		    SerializeHeader(context);
		    SerializeObjects(context);
	    }

	    private Block(BlockDeserializeContext context, IHashProvider hashProvider)
	    {
		    DeserializeHeader(context);
		    DeserializeObjects(context);
		    Hash = this.ToHashBytes(hashProvider);
	    }

	    public void DeserializeHeader(BlockDeserializeContext context)
	    {
		    context.br.ReadInt32();                     // Version
		    PreviousHash = context.br.ReadBuffer();     // PreviousHash
		    Timestamp = context.br.ReadInt64();         // Timestamp
		    context.br.ReadDouble();                    // Difficulty
		    Nonce = context.br.ReadInt64();             // Nonce
	    }

		public void DeserializeObjects(BlockDeserializeContext context)
	    {
		    var count = context.br.ReadInt32();
		    Objects = new List<BlockObject>(count);
		    for (var i = 0; i < count; i++)
				Objects.Add(new BlockObject(context));
		}
		
	    public void SerializeHeader(BlockSerializeContext context)
	    {
		    context.bw.Write(context.Version);          // Version
			context.bw.WriteBuffer(PreviousHash);       // PreviousHash
			context.bw.Write(Timestamp);                // Timestamp
			context.bw.Write(GetDifficulty());          // Difficulty
			context.bw.Write(Nonce);                    // Nonce
		}

	    public void SerializeObjects(BlockSerializeContext context)
	    {
		    var count = Objects?.Count ?? 0;
		    context.bw.Write(count);
		    if (Objects != null)
				foreach (var @object in Objects)
					@object.Serialize(context);
		}
		
		public void RoundTripCheck(IHashProvider hashProvider, IBlockObjectTypeProvider typeProvider)
	    {
		    // Serialize a first time
		    var firstMemoryStream = new MemoryStream();
		    var firstSerializeContext = new BlockSerializeContext(new BinaryWriter(firstMemoryStream), typeProvider);
		    Serialize(firstSerializeContext);
		    var originalData = firstMemoryStream.ToArray();

		    // Then deserialize that data
		    var br = new BinaryReader(new MemoryStream(originalData));
		    var deserializeContext = new BlockDeserializeContext(br, typeProvider);
		    var deserialized = new Block(deserializeContext, hashProvider);

		    // Then serialize that deserialized data and see if it matches
		    var secondMemoryStream = new MemoryCompareStream(originalData);
		    var secondSerializeContext = new BlockSerializeContext(new BinaryWriter(secondMemoryStream), typeProvider);
		    deserialized.Serialize(secondSerializeContext);
	    }

	    #endregion
	}
}
