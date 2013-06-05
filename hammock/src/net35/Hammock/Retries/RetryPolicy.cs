using System;
using System.Collections.Generic;
using System.Linq;
using Hammock.Extensions;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RetryPolicy
    {
        public virtual ICollection<IRetryCondition> RetryConditions { get; set; }
        
        public virtual int RetryCount { get; set; }
        
        public RetryPolicy()
        {
            Initialize();
        }

        private void Initialize()
        {
            RetryConditions = new List<IRetryCondition>(0);
        }

        public virtual void RetryOn(IEnumerable<IRetryCondition> conditions)
        {
            foreach(var condition in conditions)
            {
                RetryConditions.Add(condition);    
            }
        }

        public virtual void RetryOn(params IRetryCondition[] conditions)
        {
            var enumerable = conditions.ToList();
            RetryOn(enumerable);
        }
    }
}
