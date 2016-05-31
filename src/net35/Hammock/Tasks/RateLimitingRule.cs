using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RateLimitingRule<T> : IRateLimitingRule<T>
    {
        private readonly RateLimitType _rateLimitType;

        public RateLimitingRule(Predicate<T> rateLimitIf)
        {
            _rateLimitType = RateLimitType.ByPredicate;
            RateLimitIf = rateLimitIf;
        }

        public RateLimitingRule(double percentOfTotal)
        {
            _rateLimitType = RateLimitType.ByPercent;
            LimitToPercentOfTotal = percentOfTotal;
        }

        public RateLimitingRule(Func<T> getRateLimitStatus, Predicate<T> rateLimitIf)
        {
            _rateLimitType = RateLimitType.ByPredicate;
            GetRateLimitStatus = getRateLimitStatus;
            RateLimitIf = rateLimitIf;
        }

        public RateLimitingRule(Func<T> getRateLimitStatus, double percentOfTotal)
        {
            _rateLimitType = RateLimitType.ByPercent;
            GetRateLimitStatus = getRateLimitStatus;
            LimitToPercentOfTotal = percentOfTotal;
        }

        #region IRateLimitingRule Members

        public virtual double? LimitToPercentOfTotal { get; private set; }
        public virtual RateLimitType RateLimitType
        {
            get { return _rateLimitType; }
        }

        public Func<T> GetRateLimitStatus { get; set; }
        public Predicate<T> RateLimitIf { get; private set; }

        #endregion

        public bool ShouldSkipForRateLimiting()
        {
            // [JD]: Only pre-skip via predicate; percentage based adjusts rate after the call
            if (RateLimitType != RateLimitType.ByPredicate)
            {
                return false;
            }

            if (RateLimitIf == null)
            {
                throw new InvalidOperationException("Rule is set to use predicate, but no predicate is defined.");
            }

            var status = default(T);
            if (GetRateLimitStatus != null)
            {
                status = GetRateLimitStatus();
            }
            return !RateLimitIf(status);
        }

        public TimeSpan? CalculateNewInterval()
        {
            if (RateLimitType != RateLimitType.ByPercent)
            {
                return null;
            }

            if (!LimitToPercentOfTotal.HasValue)
            {
                return null;
            }
            var currentRateLimit = (IRateLimitStatus)GetRateLimitStatus();
            if (currentRateLimit.RemainingUses == 0)
            {
                return currentRateLimit.NextReset - DateTime.Now;
            }
            var secondsUntilNextReset = (currentRateLimit.NextReset - DateTime.Now).TotalSeconds;
            var desiredRetriesBeforeReset = currentRateLimit.RemainingUses * LimitToPercentOfTotal.Value;
            var desiredInterval = (int)Math.Floor(secondsUntilNextReset / desiredRetriesBeforeReset);
            return new TimeSpan(0, 0, 0, desiredInterval);
        }
    }
}