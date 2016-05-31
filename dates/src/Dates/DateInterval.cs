using System;

namespace Dates
{
    /// <summary>
    /// Used when calculating the difference between two <see cref="DateTime" /> instances
    /// with the <see cref="DateSpan" /> class.
    /// </summary>
    public enum DateInterval
    {
        /// <summary>
        /// Years
        /// </summary>
        Years,
        ///<summary>
        /// Months
        ///</summary>
        Months,
        /// <summary>
        /// Weeks
        /// </summary>
        Weeks,
        /// <summary>
        /// Days
        /// </summary>
        Days,
        /// <summary>
        /// Hours
        /// </summary>
        Hours,
        /// <summary>
        /// Minutes
        /// </summary>
        Minutes,
        /// <summary>
        /// Seconds
        /// </summary>
        Seconds
    }
}