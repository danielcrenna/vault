using ChainLib.Models;

namespace ChainLib.Configuration
{
	public class ChainSettings
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string StorageEngine { get; set; }
		public string StorageDirectory { get; set; }
		public Block GenesisBlock { get; set; } 

		[Computed]
		public byte[] Hash { get; set; }
	}
}