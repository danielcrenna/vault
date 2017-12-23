namespace ChainLib.Models
{
	public class NoProofOfWork : IProofOfWork
	{
		public double GetDifficulty(long index)
		{
			return 0;
		}

		public Block ProveWorkFor(Block block, double difficulty)
		{
			block.Difficulty = 0;
			return block;
		}
	}
}