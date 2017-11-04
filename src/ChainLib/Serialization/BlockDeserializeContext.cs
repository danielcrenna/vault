using System;
using System.IO;
using ChainLib.Models;

namespace ChainLib.Serialization
{
	public class BlockDeserializeContext
	{
		public BlockDeserializeContext(BinaryReader br, IBlockObjectTypeProvider typeProvider)
		{
			this.br = br;
			this.typeProvider = typeProvider;

			Version = br.ReadInt32();

			if (Version > BlockSerializeContext.formatVersion)
				throw new Exception("Tried to load block with a version that is too new");
		}

		public readonly BinaryReader br;
		public readonly IBlockObjectTypeProvider typeProvider;

		public int Version { get; private set; }
	}
}