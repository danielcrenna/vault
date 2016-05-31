using System;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RetryCustomCondition<T> : 
        IRetryCustomCondition,
        IRetryCondition<T>
    {
        public virtual Func<T> ConditionFunction { get; set; }
        public virtual Predicate<T> RetryIf
        {
            get
            {
                return t => false;
            }
        }
    }
}
