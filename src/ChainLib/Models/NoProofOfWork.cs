namespace ChainLib.Models
{
	public class NoProofOfWork : IProofOfWork
	{
		public uint GetDifficulty(long index)
		{
			return 0;
		}

		public Block ProveWorkFor(Block block, uint difficulty)
		{
			block.Difficulty = 0;
			return block;
		}
	}
}