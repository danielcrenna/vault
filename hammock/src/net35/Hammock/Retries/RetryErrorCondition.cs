using System;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RetryErrorCondition : IRetryCondition<Exception>
    {
        public virtual Predicate<Exception> RetryIf
        {
            get { return e => false; }
        }
    }
}