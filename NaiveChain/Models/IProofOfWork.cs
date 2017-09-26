namespace NaiveChain.Models
{
    public interface IProofOfWork
    {
        double GetDifficulty(long index);
	    Block ProveWorkFor(Block block, double difficulty);
    }
}