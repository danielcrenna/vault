using System;
using metrics.Support;

namespace metrics.Core
{
    public interface IMetered
    {
        /// <summary>
        /// Returns the meter's rate unit
        /// </summary>
        /// <returns></returns>
        TimeUnit RateUnit();

       /// <summary>
        /// Returns the type of events the meter is measuring
       /// </summary>
       /// <returns></returns>
        String EventType();

        /// <summary>
        ///  Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        long Count();

        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        double FifteenMinuteRate();

        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        double FiveMinuteRate();

        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        double MeanRate();

        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        double OneMinuteRate();
    }
}
