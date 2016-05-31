using System;

namespace Hammock.Tasks
{


#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions<T> : TaskOptions, ITaskOptions<T>
    {
        private RateLimitType _rateLimitType = RateLimitType.ByPredicate;
        private double? _rateLimitPercent; 

        public virtual RateLimitType RateLimitType { get { return _rateLimitType; } }
        public virtual Predicate<T> RateLimitingPredicate { get; set; }
        public virtual Func<T> GetRateLimitStatus { get; set; }
        public virtual double? RateLimitPercent
        {
            get { return _rateLimitPercent; }
            set
            {
                if ( value != null)
                {
                    _rateLimitType = RateLimitType.ByPercent;
                }
                else
                {
                    _rateLimitType = RateLimitType.ByPredicate;
                }
                _rateLimitPercent = value;
            }
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions : ITaskOptions
    {
        public virtual TimeSpan DueTime { get; set; }
        public virtual int RepeatTimes { get; set; }
        public virtual TimeSpan RepeatInterval { get; set; }
        public virtual bool ContinueOnError { get; set; }
    }
}
