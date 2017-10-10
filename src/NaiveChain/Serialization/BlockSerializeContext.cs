using System;
using System.IO;

namespace NaiveChain.Serialization
{
	public partial class BlockSerializeContext
	{
		public BlockSerializeContext(BinaryWriter bw, int version = formatVersion)
		{
			this.bw = bw;
			if (Version > formatVersion)
				throw new Exception("Tried to save block with a version that is too new");
			Version = version;

			bw.Write(Version);
		}

		public readonly BinaryWriter bw;

		#region Version

		public const int formatVersion = 1;

		public int Version { get; private set; }

		#endregion
	}
}