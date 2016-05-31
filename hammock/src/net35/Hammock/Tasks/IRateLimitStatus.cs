using System;


namespace Hammock.Tasks
{
    /// <summary>
    /// Describes the current status of a rate limited API or other resource
    /// <remarks>Based mostly on Twitter's rate limit status</remarks>
    /// </summary>
    public interface IRateLimitStatus
    {
        /// <summary>
        /// Gets the current number of resource uses available
        /// </summary>
        int RemainingUses { get; }
        /// <summary>
        /// Gets the next time the <see cref="RemainingUses"/> is due to be refreshed
        /// </summary>
        DateTime NextReset { get; }
    }
}
