using System;
using System.IO;

namespace NaiveChain.Serialization
{
	public class BlockDeserializeContext
	{
		public BlockDeserializeContext(BinaryReader br)
		{
			this.br = br;

			Version = br.ReadInt32();

			if (Version > BlockSerializeContext.formatVersion)
				throw new Exception("Tried to load block with a version that is too new");
		}

		public readonly BinaryReader br;

		public int Version { get; private set; }
	}
}