namespace ChainLib.Models
{
	public interface IBlockDescriptor
	{
		int Version { get; set; }
		byte[] PreviousHash { get; set; }
		byte[] MerkleRootHash { get; set; }
		uint Timestamp { get; set; }
		uint Difficulty { get; set; }
		long Nonce { get; set; }
	}
}