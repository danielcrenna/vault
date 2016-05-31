using Dates;

namespace linger
{
    public class Every
    {
        private readonly ScheduledJob _job;
        private readonly int _n;

        public Every(ScheduledJob job) : this(job, 1)
        {
            _job = job;
        }

        public Every(ScheduledJob job, int n)
        {
            _job = job;
            _n = n;
        }

        public RepeatInfo Second()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Seconds, _n));
        }
        
        public RepeatInfo Minute()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Minutes, _n));
        }

        public RepeatInfo Hour()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Hours, _n));
        }

        public RepeatInfo Day()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Days, _n));
        }

        public RepeatInfo Month()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Months, _n));
        }

        public RepeatInfo Year()
        {
            return new RepeatInfo(_job.RunAt, new DatePeriod(DatePeriodFrequency.Years, _n));
        }
    }
}