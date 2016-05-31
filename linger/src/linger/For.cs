using Dates;

namespace linger
{
    public class For
    {
        private readonly RepeatInfo _info;
        private readonly int _count;

        public For(RepeatInfo info, int count)
        {
            _info = info;
            _count = count;
        }

        public RepeatInfo Seconds()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Seconds, _count);
            return _info;
        }

        public RepeatInfo Minutes()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Minutes, _count);
            return _info;
        }

        public RepeatInfo Hours()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Hours, _count);
            return _info;
        }

        public RepeatInfo Days()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Days, _count);
            return _info;
        }

        public RepeatInfo Weeks()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Weeks, _count);
            return _info;
        }

        public RepeatInfo Months()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Months, _count);
            return _info;
        }

        public RepeatInfo Years()
        {
            _info.EndPeriod = new DatePeriod(DatePeriodFrequency.Years, _count);
            return _info;
        }

        public RepeatInfo IncludingWeekends()
        {
            _info.IncludeWeekends = true;
            return _info;
        }

        public RepeatInfo ExcludingWeekends()
        {
            _info.IncludeWeekends = false;
            return _info;
        }
    }
}