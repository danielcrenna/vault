using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Dates;

namespace linger
{
    public class RepeatInfo
    {
        private static readonly IDictionary<string, IEnumerable<DateTime>> Cache = new Dictionary<string, IEnumerable<DateTime>>(); 

        [IgnoreDataMember]
        public DatePeriod Period { get; set; }

        [IgnoreDataMember]
        public DatePeriod? EndPeriod { get; set; }

        [IgnoreDataMember]
        public IEnumerable<DateTime> AllOccurrences
        {
            get { return GetSeriesOccurrences(); }
        }

        public DatePeriodFrequency PeriodFrequency { get { return Period.Frequency; } }
        public int PeriodQuantifier { get { return Period.Quantifier; } }

        public DatePeriodFrequency? EndPeriodFrequency { get { return EndPeriod.HasValue ? EndPeriod.Value.Frequency : default(DatePeriodFrequency?); }}
        public int? EndPeriodQuantifier { get { return EndPeriod.HasValue ? EndPeriod.Value.Quantifier : default(int?); }}

        public DateTime? Start { get; set; }
        public bool IncludeWeekends { get; set; }

        internal RepeatInfo(DateTime? start, DatePeriod period)
        {
            Start = start;
            Period = period;
        }

        public DateTime? NextOccurrence
        {
            get
            {
                if(!EndPeriod.HasValue)
                {
                    return GetSeriesOccurrences(new DatePeriod(DatePeriodFrequency.Years, 100)).FirstOrDefault();
                }
                var occurrence = GetSeriesOccurrences().FirstOrDefault();
                return occurrence;
            }
        }

        public DateTime? LastOccurrence
        {
            get { return EndPeriod.HasValue ? GetSeriesOccurrences().Last() : (DateTime?)null; }
        }
       
        private IEnumerable<DateTime> GetSeriesOccurrences(DatePeriod? endPeriod = null)
        {
            endPeriod = endPeriod.HasValue ? endPeriod.Value : EndPeriod.HasValue ? EndPeriod.Value : (DatePeriod?)null;
            if (!endPeriod.HasValue)
            {
                throw new ArgumentException("You cannot request the occurrences for an infinite series");
            }

            var cacheKey = string.Format(
                "frequency:{0}_quantifier:{1}_end:{2}_start:{3}_skip:{4}", Period.Frequency,
                Period.Quantifier,
                EndPeriod.HasValue ? string.Format("{0}_{1}", EndPeriod.Value.Frequency, EndPeriod.Value.Quantifier) : null,
                Start,
                !IncludeWeekends
            );

            IEnumerable<DateTime> occurrences;
            if(Cache.TryGetValue(cacheKey, out occurrences))
            {
                return occurrences;
            }

            var start = Start.HasValue ? Start.Value : DateTime.Now;
            DateTime end;
            
            // Get the last occurrence
            switch (endPeriod.Value.Frequency)
            {
                case DatePeriodFrequency.Years:
                    end = start.AddYears(endPeriod.Value.Quantifier);
                    break;
                case DatePeriodFrequency.Weeks:
                    end = start.AddDays(endPeriod.Value.Quantifier * 7);
                    break;
                case DatePeriodFrequency.Days:
                    end = start.AddDays(endPeriod.Value.Quantifier);
                    break;
                case DatePeriodFrequency.Hours:
                    end = start.AddHours(endPeriod.Value.Quantifier);
                    break;
                case DatePeriodFrequency.Minutes:
                    end = start.AddHours(endPeriod.Value.Quantifier);
                    break;
                case DatePeriodFrequency.Seconds:
                    end = start.AddSeconds(endPeriod.Value.Quantifier);
                    break;
                case DatePeriodFrequency.Months:
                    end = start.AddMonths(endPeriod.Value.Quantifier);
                    break;
                default:
                    throw new ArgumentException("DatePeriodFrequency");
            }
            occurrences = Period.GetOccurrences(start, end, !IncludeWeekends).ToList();
            Cache.Add(cacheKey, occurrences);
            return occurrences;
        }
    }
}