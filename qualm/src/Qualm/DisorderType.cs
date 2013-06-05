namespace Qualm
{
    public enum DisorderType
    {
        /// <summary>
        /// Disorder algorithm according to Claude Shannon
        /// </summary>
        ShannonEntropy,
        /// <summary>
        /// The probability that an outcome is mispredicted
        /// </summary>
        GiniImpurity,
        /// <summary>
        /// The highest probability that an outcome is mispredicted
        /// </summary>
        ClassificationError
    }
}