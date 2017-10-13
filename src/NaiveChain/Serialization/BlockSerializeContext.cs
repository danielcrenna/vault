using System;
using System.IO;
using NaiveChain.Models;

namespace NaiveChain.Serialization
{
	public partial class BlockSerializeContext
	{
		public BlockSerializeContext(BinaryWriter bw, IBlockObjectTypeProvider typeProvider, int version = formatVersion)
		{
			this.bw = bw;
			this.typeProvider = typeProvider;
			if (Version > formatVersion)
				throw new Exception("Tried to save block with a version that is too new");
			Version = version;

			bw.Write(Version);
		}

		public readonly BinaryWriter bw;
		public readonly IBlockObjectTypeProvider typeProvider;

		#region Version

		public const int formatVersion = 1;

		public int Version { get; private set; }

		#endregion
	}
}