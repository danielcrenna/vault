using System.Collections.Generic;
using System.Linq;

namespace copper
{
    public class RetryPolicy
    {
        private readonly IDictionary<int, RetryDecision> _rules;

        public RetryPolicy()
        {
            _rules = new Dictionary<int, RetryDecision>();
        }

        public void After(int tries, RetryDecision action)
        {
            _rules.Add(tries, action);
        }

        public RetryDecision DecideOn<T>(T @event, int attempts)
        {
            foreach (var threshold in _rules.Keys.OrderBy(k => k).Where(threshold => attempts >= threshold))
            {
                return _rules[threshold];
            }
            return RetryDecision.Requeue;
        }
    }
}