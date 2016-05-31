namespace ab
{
    public interface IExperimentType
    {
        double ProbabilityOfScore(double score);
        Score Score(Experiment experiment, double probability = 90.0);
        string Conclusion(Score score, double probability = 90.0);
    }
}