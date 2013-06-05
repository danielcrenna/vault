namespace Qualm
{
    /// <summary>
    /// Represents strategies for calculating distance metrics
    /// </summary>
    public enum DistanceType
    {
        /// <summary>
        /// Using a direct path from each point
        /// </summary>
        Euclidean,
        /// <summary>
        /// Using a "taxicab" path from each point; may only follow one axis at a time
        /// </summary>
        Manhattan,
        /// <summary>
        /// Using a harmonization of Euclidean (direct), Manhattan (horizontal-vertical), and Chebyshev (diagonals) metrics
        /// </summary>
        Minkowski,
        /// <summary>
        /// Using the number of substitutions required to make one sequence identical to the other
        /// </summary>
        Hanning
    }
}