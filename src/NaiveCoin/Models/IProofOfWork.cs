namespace NaiveCoin.Models
{
    public interface IProofOfWork
    {
        double GetDifficulty(long index);
	    CurrencyBlock ProveWorkFor(CurrencyBlock block, double difficulty);
    }
}