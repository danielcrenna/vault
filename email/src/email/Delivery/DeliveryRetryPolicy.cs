using System.Collections.Generic;
using System.Linq;

namespace email.Delivery
{
    /// <summary>
    /// A place to define how to handle a failed email
    /// </summary>
    public class DeliveryRetryPolicy
    {
        private readonly IDictionary<int, DeliveryRetryDecision> _rules;

        public DeliveryRetryPolicy()
        {
            _rules = new Dictionary<int, DeliveryRetryDecision>();    
        }

        public void After(int tries, DeliveryRetryDecision action)
        {
            _rules.Add(tries, action);
        }

        public DeliveryRetryDecision DecideOn(EmailMessage message)
        {
            if(message == null)
            {
                return DeliveryRetryDecision.Destroy;
            }
            foreach (var threshold in _rules.Keys.OrderBy(k => k).Where(threshold => message.DeliveryAttempts >= threshold))
            {
                return _rules[threshold];
            }
            return DeliveryRetryDecision.SendToBackOfQueue;
        }
    }
}