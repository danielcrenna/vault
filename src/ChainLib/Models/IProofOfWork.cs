namespace ChainLib.Models
{
    public interface IProofOfWork
    {
        uint GetDifficulty(long index);
	    Block ProveWorkFor(Block block, uint difficulty);
    }
}