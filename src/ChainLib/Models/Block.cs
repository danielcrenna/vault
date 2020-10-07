using System.Collections.Generic;
using System.IO;
using ChainLib.Serialization;
using Sodium;

namespace ChainLib.Models
{
    public class Block : IBlockDescriptor
	{
	    internal static IReadOnlyCollection<BlockObject> NoObjects = new List<BlockObject>(0);

		public Block()
	    {
		    Version = 1;
			Objects = new List<BlockObject>();
	    }

		public long? Index { get; set; }
		public int Version { get; set; }
        public byte[] PreviousHash { get; set; }
		public byte[] MerkleRootHash { get; set; }
        public uint Timestamp { get; set; }
	    public uint Difficulty { get; set; }
		public long Nonce { get; set; }

        public IList<BlockObject> Objects { get; set; }

		[Computed]
		public byte[] Hash { get; set; }

	    #region Serialization

	    public void Serialize(BlockSerializeContext context)
	    {
		    SerializeHeader(context);
		    context.bw.WriteBuffer(Hash);
			SerializeObjects(context);
	    }

	    private Block(BlockDeserializeContext context)
	    {
		    DeserializeHeader(context);
		    Hash = context.br.ReadBuffer();
			DeserializeObjects(context);
	    }

	    public void DeserializeHeader(BlockDeserializeContext context)
	    {
		    BlockHeader.Deserialize(this, context);
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
		    BlockHeader.Serialize(this, context);
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

		    byte[] nonce;
		    if (typeProvider.SecretKey != null)
		    {
			    nonce = StreamEncryption.GenerateNonceChaCha20();
				firstSerializeContext.bw.WriteBuffer(nonce);
			    using (var ems = new MemoryStream())
			    {
					using (var ebw = new BinaryWriter(ems))
					{
						var ec = new BlockSerializeContext(ebw, typeProvider, firstSerializeContext.Version);
						Serialize(ec);
						firstSerializeContext.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
					}
			    }
			}
		    else
		    {
			    firstSerializeContext.bw.Write(false);
			    Serialize(firstSerializeContext);
			}

		    byte[] originalData = firstMemoryStream.ToArray();

			// Then deserialize that data
		    {
			    var br = new BinaryReader(new MemoryStream(originalData));
			    var deserializeContext = new BlockDeserializeContext(br, typeProvider);
			    nonce = deserializeContext.br.ReadBuffer();

			    Block deserialized;
			    if (nonce != null)
			    {
				    using (var dms = new MemoryStream(StreamEncryption.DecryptChaCha20(deserializeContext.br.ReadBuffer(), nonce, typeProvider.SecretKey)))
				    {
					    using (var dbr = new BinaryReader(dms))
					    {
						    var dc = new BlockDeserializeContext(dbr, typeProvider);
						    deserialized = new Block(dc);
					    }
				    }
				}
			    else
			    {
				    deserialized = new Block(deserializeContext);
			    }

				// Then serialize that deserialized data and see if it matches
			    {
				    var secondMemoryStream = new MemoryCompareStream(originalData);
				    var secondSerializeContext = new BlockSerializeContext(new BinaryWriter(secondMemoryStream), typeProvider);
				    if (typeProvider.SecretKey != null)
				    {
					    secondSerializeContext.bw.WriteBuffer(nonce);
					    using (var ems = new MemoryStream())
					    {
						    using (var ebw = new BinaryWriter(ems))
						    {
							    var ec = new BlockSerializeContext(ebw, typeProvider, secondSerializeContext.Version);
							    deserialized.Serialize(ec);
							    secondSerializeContext.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
						    }
					    }
				    }
				    else
				    {
					    secondSerializeContext.bw.Write(false);
					    deserialized.Serialize(secondSerializeContext);
					}
				}
			}
	    }

	    #endregion
	}
}
